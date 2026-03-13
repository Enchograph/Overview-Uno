namespace Overview.Server.Infrastructure.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string Issuer { get; init; } = "Overview.Server";

    public string Audience { get; init; } = "Overview.Client";

    public bool RequireConfirmedEmail { get; init; } = true;

    public string JwtSigningKey { get; init; } = "development-signing-key-change-me-1234567890";

    public int AccessTokenLifetimeMinutes { get; init; } = 60;

    public int RefreshTokenLifetimeDays { get; init; } = 30;

    public int VerificationCodeLifetimeMinutes { get; init; } = 10;
}
