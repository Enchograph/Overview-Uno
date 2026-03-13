namespace Overview.Client.Infrastructure.Api.Auth.Contracts;

public sealed class AuthRegisterRequest
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string VerificationCode { get; init; } = string.Empty;
}
