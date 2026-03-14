namespace Overview.Client.Application.Auth;

public sealed class AuthSession
{
    public AuthenticationMode Mode { get; init; } = AuthenticationMode.Remote;

    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public string BaseUrl { get; init; } = string.Empty;

    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public DateTimeOffset AccessTokenExpiresAt { get; init; }

    public DateTimeOffset? RestoredAt { get; init; }

    public bool SupportsRemoteSync => Mode == AuthenticationMode.Remote;
}
