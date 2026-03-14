namespace Overview.Client.Application.Auth;

public interface IAuthenticationService
{
    AuthSession? CurrentSession { get; }

    bool IsAuthenticated { get; }

    Task<VerificationCodeDispatchResult> SendVerificationCodeAsync(
        string baseUrl,
        string email,
        CancellationToken cancellationToken = default);

    Task<AuthSession> RegisterAsync(
        string baseUrl,
        string email,
        string password,
        string verificationCode,
        CancellationToken cancellationToken = default);

    Task<AuthSession> LoginAsync(
        string baseUrl,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthSession> LoginOfflineAsync(CancellationToken cancellationToken = default);

    Task<AuthSession?> RestoreSessionAsync(CancellationToken cancellationToken = default);

    Task<AuthSession> RefreshSessionAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
