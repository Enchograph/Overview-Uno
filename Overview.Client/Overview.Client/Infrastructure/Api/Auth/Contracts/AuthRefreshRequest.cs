namespace Overview.Client.Infrastructure.Api.Auth.Contracts;

public sealed class AuthRefreshRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
