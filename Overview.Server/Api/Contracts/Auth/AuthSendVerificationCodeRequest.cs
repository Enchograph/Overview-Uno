namespace Overview.Server.Api.Contracts.Auth;

public sealed class AuthSendVerificationCodeRequest
{
    public string Email { get; init; } = string.Empty;
}
