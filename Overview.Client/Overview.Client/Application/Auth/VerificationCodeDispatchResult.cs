namespace Overview.Client.Application.Auth;

public sealed class VerificationCodeDispatchResult
{
    public required string Email { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}
