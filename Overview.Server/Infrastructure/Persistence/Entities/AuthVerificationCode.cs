namespace Overview.Server.Infrastructure.Persistence.Entities;

public sealed class AuthVerificationCode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public AuthVerificationCodePurpose Purpose { get; set; } = AuthVerificationCodePurpose.Registration;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ConsumedAt { get; set; }
}
