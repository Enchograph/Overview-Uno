using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Overview.Server.Infrastructure.Configuration;
using Overview.Server.Infrastructure.Diagnostics;
using Overview.Server.Infrastructure.Persistence;
using Overview.Server.Infrastructure.Persistence.Entities;

namespace Overview.Server.Infrastructure.Identity;

public sealed class VerificationCodeService : IVerificationCodeService
{
    private readonly OverviewDbContext _dbContext;
    private readonly AuthenticationOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly IOverviewLogger _logger;

    public VerificationCodeService(
        OverviewDbContext dbContext,
        IOptions<AuthenticationOptions> options,
        TimeProvider timeProvider,
        IOverviewLoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = loggerFactory.CreateLogger<VerificationCodeService>();
    }

    public async Task<AuthVerificationCode> CreateRegistrationCodeAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        var existingCodes = await _dbContext.VerificationCodes
            .Where(record => record.Email == email && record.ConsumedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var record in existingCodes)
        {
            record.ConsumedAt = now;
        }

        var authVerificationCode = new AuthVerificationCode
        {
            Email = email,
            CodeHash = HashCode(code),
            Purpose = AuthVerificationCodePurpose.Registration,
            ExpiresAt = now.AddMinutes(_options.VerificationCodeLifetimeMinutes),
            CreatedAt = now
        };

        _dbContext.VerificationCodes.Add(authVerificationCode);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registration verification code for {Email}: {Code}", email, code);

        return authVerificationCode;
    }

    public async Task<AuthVerificationCode?> ValidateRegistrationCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var codeHash = HashCode(code);

        return await _dbContext.VerificationCodes
            .Where(record => record.Email == email
                && record.Purpose == AuthVerificationCodePurpose.Registration
                && record.CodeHash == codeHash
                && record.ConsumedAt == null
                && record.ExpiresAt > now)
            .OrderByDescending(record => record.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string HashCode(string code)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(code.Trim()));
        return Convert.ToHexString(hashBytes);
    }
}
