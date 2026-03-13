namespace Overview.Client.Infrastructure.Api.Auth.Contracts;

public sealed class AuthLoginRequest
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
