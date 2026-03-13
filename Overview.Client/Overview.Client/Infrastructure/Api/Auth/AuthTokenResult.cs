namespace Overview.Client.Infrastructure.Api.Auth;

public sealed class AuthTokenResult
{
    public required Guid UserId { get; init; }

    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}
