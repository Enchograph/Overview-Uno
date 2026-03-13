using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Overview.Server.Infrastructure.Configuration;

namespace Overview.Server.Infrastructure.Identity;

public sealed class JwtAuthTokenService : IAuthTokenService
{
    private readonly AuthenticationOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _signingCredentials;

    public JwtAuthTokenService(
        IOptions<AuthenticationOptions> options,
        TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;

        var keyBytes = Encoding.UTF8.GetBytes(_options.JwtSigningKey);
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);
    }

    public AuthTokenPair CreateTokenPair(Guid userId, string email)
    {
        var now = _timeProvider.GetUtcNow();
        var accessTokenExpiresAt = now.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var refreshTokenExpiresAt = now.AddDays(_options.RefreshTokenLifetimeDays);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = accessTokenExpiresAt.UtcDateTime,
            NotBefore = now.UtcDateTime,
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));

        Span<byte> refreshTokenBytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(refreshTokenBytes);
        var refreshToken = Convert.ToBase64String(refreshTokenBytes);

        return new AuthTokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    public string HashRefreshToken(string refreshToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hashBytes);
    }
}
