using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Overview.Server.Api.Contracts.Auth;
using Overview.Server.Infrastructure.Configuration;
using Overview.Server.Infrastructure.Identity;
using Overview.Server.Infrastructure.Persistence;
using Overview.Server.Infrastructure.Persistence.Entities;

namespace Overview.Server.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly OverviewDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenService _authTokenService;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly TimeProvider _timeProvider;
    private readonly AuthenticationOptions _authenticationOptions;

    public AuthController(
        OverviewDbContext dbContext,
        IPasswordHasher passwordHasher,
        IAuthTokenService authTokenService,
        IVerificationCodeService verificationCodeService,
        TimeProvider timeProvider,
        IOptions<AuthenticationOptions> authenticationOptions)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _authTokenService = authTokenService;
        _verificationCodeService = verificationCodeService;
        _timeProvider = timeProvider;
        _authenticationOptions = authenticationOptions.Value;
    }

    [HttpPost("send-verification-code")]
    [ProducesResponseType<AuthSendVerificationCodeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthSendVerificationCodeResponse>> SendVerificationCodeAsync(
        [FromBody] AuthSendVerificationCodeRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (email is null)
        {
            return ValidationProblem("A valid email address is required.");
        }

        var codeRecord = await _verificationCodeService.CreateRegistrationCodeAsync(email, cancellationToken);
        return Ok(new AuthSendVerificationCodeResponse
        {
            Email = email,
            ExpiresAt = codeRecord.ExpiresAt
        });
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthTokenResponse>> RegisterAsync(
        [FromBody] AuthRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (email is null)
        {
            return ValidationProblem("A valid email address is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblem("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.VerificationCode))
        {
            return ValidationProblem("Verification code is required.");
        }

        var existingUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

        if (existingUser is not null)
        {
            return Conflict(new ProblemDetails { Title = "Account already exists." });
        }

        var codeRecord = await _verificationCodeService.ValidateRegistrationCodeAsync(
            email,
            request.VerificationCode,
            cancellationToken);

        if (codeRecord is null)
        {
            return ValidationProblem("Verification code is invalid or expired.");
        }

        var now = _timeProvider.GetUtcNow();
        var user = new AuthUser
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsEmailConfirmed = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Users.Add(user);
        codeRecord.ConsumedAt = now;

        var tokenPair = _authTokenService.CreateTokenPair(user.Id, email);
        _dbContext.RefreshTokens.Add(new AuthRefreshToken
        {
            UserId = user.Id,
            TokenHash = _authTokenService.HashRefreshToken(tokenPair.RefreshToken),
            ExpiresAt = tokenPair.RefreshTokenExpiresAt,
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(user.Id, tokenPair));
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> LoginAsync(
        [FromBody] AuthLoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (email is null || string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblem("Email and password are required.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ProblemDetails { Title = "Invalid email or password." });
        }

        if (_authenticationOptions.RequireConfirmedEmail && !user.IsEmailConfirmed)
        {
            return Unauthorized(new ProblemDetails { Title = "Email address is not confirmed." });
        }

        var now = _timeProvider.GetUtcNow();
        user.LastLoginAt = now;
        user.UpdatedAt = now;

        var tokenPair = _authTokenService.CreateTokenPair(user.Id, email);
        _dbContext.RefreshTokens.Add(new AuthRefreshToken
        {
            UserId = user.Id,
            TokenHash = _authTokenService.HashRefreshToken(tokenPair.RefreshToken),
            ExpiresAt = tokenPair.RefreshTokenExpiresAt,
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(user.Id, tokenPair));
    }

    [HttpPost("refresh")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> RefreshAsync(
        [FromBody] AuthRefreshRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ValidationProblem("Refresh token is required.");
        }

        var refreshTokenHash = _authTokenService.HashRefreshToken(request.RefreshToken);
        var now = _timeProvider.GetUtcNow();

        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == refreshTokenHash
                    && token.RevokedAt == null
                    && token.ExpiresAt > now,
                cancellationToken);

        if (storedToken?.User is null)
        {
            return Unauthorized(new ProblemDetails { Title = "Refresh token is invalid or expired." });
        }

        storedToken.RevokedAt = now;
        storedToken.ReplacedByTokenHash = string.Empty;

        var tokenPair = _authTokenService.CreateTokenPair(storedToken.User.Id, storedToken.User.Email);
        var replacementTokenHash = _authTokenService.HashRefreshToken(tokenPair.RefreshToken);
        storedToken.ReplacedByTokenHash = replacementTokenHash;

        _dbContext.RefreshTokens.Add(new AuthRefreshToken
        {
            UserId = storedToken.User.Id,
            TokenHash = replacementTokenHash,
            ExpiresAt = tokenPair.RefreshTokenExpiresAt,
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(storedToken.User.Id, tokenPair));
    }

    private static ActionResult ValidationProblem(string detail)
    {
        return new BadRequestObjectResult(new ProblemDetails
        {
            Title = "Validation failed.",
            Detail = detail
        });
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        try
        {
            return new System.Net.Mail.MailAddress(email.Trim()).Address.ToLowerInvariant();
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static AuthTokenResponse ToResponse(Guid userId, AuthTokenPair tokenPair)
    {
        return new AuthTokenResponse
        {
            UserId = userId,
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            ExpiresAt = tokenPair.AccessTokenExpiresAt
        };
    }
}
