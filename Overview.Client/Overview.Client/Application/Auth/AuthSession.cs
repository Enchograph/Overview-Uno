namespace Overview.Client.Application.Auth;

public sealed class AuthSession
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required string BaseUrl { get; init; }

    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset AccessTokenExpiresAt { get; init; }

    public DateTimeOffset? RestoredAt { get; init; }
}
