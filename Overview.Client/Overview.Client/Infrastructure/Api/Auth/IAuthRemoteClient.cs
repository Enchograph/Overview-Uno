namespace Overview.Client.Infrastructure.Api.Auth;

using Overview.Client.Application.Auth;

public interface IAuthRemoteClient
{
    Task<VerificationCodeDispatchResult> SendVerificationCodeAsync(
        string baseUrl,
        string email,
        CancellationToken cancellationToken = default);

    Task<AuthTokenResult> RegisterAsync(
        string baseUrl,
        string email,
        string password,
        string verificationCode,
        CancellationToken cancellationToken = default);

    Task<AuthTokenResult> LoginAsync(
        string baseUrl,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthTokenResult> RefreshAsync(
        string baseUrl,
        string refreshToken,
        CancellationToken cancellationToken = default);
}
