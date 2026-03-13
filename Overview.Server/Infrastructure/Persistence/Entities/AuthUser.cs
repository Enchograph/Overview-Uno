namespace Overview.Server.Infrastructure.Persistence.Entities;

public sealed class AuthUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsEmailConfirmed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<AuthRefreshToken> RefreshTokens { get; set; } = [];
}
