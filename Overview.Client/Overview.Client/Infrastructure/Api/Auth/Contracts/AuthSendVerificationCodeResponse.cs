namespace Overview.Client.Infrastructure.Api.Auth.Contracts;

public sealed class AuthSendVerificationCodeResponse
{
    public required string Email { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}
