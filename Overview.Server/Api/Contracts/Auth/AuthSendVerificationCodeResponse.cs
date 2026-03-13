namespace Overview.Server.Api.Contracts.Auth;

public sealed class AuthSendVerificationCodeResponse
{
    public required string Email { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}
