namespace Overview.Server.Api.Contracts.Auth;

public sealed class AuthTokenResponse
{
    public required Guid UserId { get; init; }

    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}
