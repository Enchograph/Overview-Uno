namespace Overview.Server.Api.Contracts.Auth;

public sealed class AuthRefreshRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
