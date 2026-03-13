namespace Overview.Server.Infrastructure.Persistence.Entities;

public sealed class AuthUser
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Email { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public bool IsEmailConfirmed { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset? LastLoginAt { get; init; }
}
