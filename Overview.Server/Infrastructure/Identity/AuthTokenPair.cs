namespace Overview.Server.Infrastructure.Identity;

public sealed class AuthTokenPair
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset AccessTokenExpiresAt { get; init; }

    public required DateTimeOffset RefreshTokenExpiresAt { get; init; }
}
