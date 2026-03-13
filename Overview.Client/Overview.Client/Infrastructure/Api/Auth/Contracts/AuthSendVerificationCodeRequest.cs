namespace Overview.Client.Infrastructure.Api.Auth.Contracts;

public sealed class AuthSendVerificationCodeRequest
{
    public string Email { get; init; } = string.Empty;
}
