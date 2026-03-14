using Overview.Client.Infrastructure.Api.Auth;
using Overview.Client.Infrastructure.Diagnostics;
using Overview.Client.Infrastructure.Settings;
using Overview.Client.Application.Widgets;

namespace Overview.Client.Application.Auth;

public sealed class AuthenticationService : IAuthenticationService
{
    private static readonly Guid OfflineUserId = Guid.Parse("f12ce825-6fcd-4d3e-98cf-47c3d7ec6f31");
    private const string OfflineEmail = "offline@overview.local";
    private readonly IAuthRemoteClient authRemoteClient;
    private readonly IAuthSessionStore authSessionStore;
    private readonly IOverviewLogger logger;
    private readonly IWidgetRefreshService widgetRefreshService;
    private readonly SemaphoreSlim sessionLock = new(1, 1);
    private AuthSession? currentSession;

    public AuthenticationService(
        IAuthRemoteClient authRemoteClient,
        IAuthSessionStore authSessionStore,
        IOverviewLoggerFactory loggerFactory,
        IWidgetRefreshService? widgetRefreshService = null)
    {
        this.authRemoteClient = authRemoteClient;
        this.authSessionStore = authSessionStore;
        logger = loggerFactory.CreateLogger<AuthenticationService>();
        this.widgetRefreshService = widgetRefreshService ?? NoOpWidgetRefreshService.Instance;
    }

    public AuthSession? CurrentSession => currentSession;

    public bool IsAuthenticated => currentSession is not null;

    public Task<VerificationCodeDispatchResult> SendVerificationCodeAsync(
        string baseUrl,
        string email,
        CancellationToken cancellationToken = default)
    {
        return authRemoteClient.SendVerificationCodeAsync(baseUrl, email, cancellationToken);
    }

    public async Task<AuthSession> RegisterAsync(
        string baseUrl,
        string email,
        string password,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        var response = await authRemoteClient.RegisterAsync(
            baseUrl,
            email,
            password,
            verificationCode,
            cancellationToken).ConfigureAwait(false);

        var session = CreateSession(baseUrl, email, response);
        await PersistSessionAsync(session, cancellationToken).ConfigureAwait(false);
        return session;
    }

    public async Task<AuthSession> LoginAsync(
        string baseUrl,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var response = await authRemoteClient.LoginAsync(
            baseUrl,
            email,
            password,
            cancellationToken).ConfigureAwait(false);

        var session = CreateSession(baseUrl, email, response);
        await PersistSessionAsync(session, cancellationToken).ConfigureAwait(false);
        return session;
    }

    public async Task<AuthSession> LoginOfflineAsync(CancellationToken cancellationToken = default)
    {
        var session = new AuthSession
        {
            Mode = AuthenticationMode.OfflineLocal,
            UserId = OfflineUserId,
            Email = OfflineEmail,
            RestoredAt = DateTimeOffset.UtcNow
        };

        await PersistSessionAsync(session, cancellationToken).ConfigureAwait(false);
        return session;
    }

    public async Task<AuthSession?> RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (currentSession is not null)
            {
                return currentSession;
            }

            var storedSession = await authSessionStore.LoadAsync(cancellationToken).ConfigureAwait(false);
            if (storedSession is null)
            {
                return null;
            }

            if (!storedSession.SupportsRemoteSync)
            {
                currentSession = CloneSession(storedSession, restoredAt: DateTimeOffset.UtcNow);
                logger.LogInformation("Offline authentication session restored for user {UserId}.", storedSession.UserId);
                await widgetRefreshService.RefreshAsync(currentSession.UserId, cancellationToken).ConfigureAwait(false);
                return currentSession;
            }

            if (storedSession.AccessTokenExpiresAt > DateTimeOffset.UtcNow)
            {
                currentSession = CloneSession(storedSession, restoredAt: DateTimeOffset.UtcNow);
                logger.LogInformation("Authentication session restored for user {UserId}.", storedSession.UserId);
                await widgetRefreshService.RefreshAsync(currentSession.UserId, cancellationToken).ConfigureAwait(false);
                return currentSession;
            }

            try
            {
                var refreshed = await RefreshSessionCoreAsync(storedSession, cancellationToken).ConfigureAwait(false);
                currentSession = refreshed;
                await authSessionStore.SaveAsync(refreshed, cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Authentication session refreshed during restore for user {UserId}.", refreshed.UserId);
                await widgetRefreshService.RefreshAsync(refreshed.UserId, cancellationToken).ConfigureAwait(false);
                return refreshed;
            }
            catch (Exception ex) when (ex is AuthRemoteException || ex is HttpRequestException)
            {
                logger.LogWarning("Failed to restore authentication session: {Message}", ex.Message);
                await authSessionStore.ClearAsync(cancellationToken).ConfigureAwait(false);
                currentSession = null;
                return null;
            }
        }
        finally
        {
            sessionLock.Release();
        }
    }

    public async Task<AuthSession> RefreshSessionAsync(CancellationToken cancellationToken = default)
    {
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var session = currentSession ?? await authSessionStore.LoadAsync(cancellationToken).ConfigureAwait(false);
            if (session is null)
            {
                throw new InvalidOperationException("No authentication session is available.");
            }

            if (!session.SupportsRemoteSync)
            {
                currentSession = CloneSession(session, restoredAt: DateTimeOffset.UtcNow);
                return currentSession;
            }

            var refreshed = await RefreshSessionCoreAsync(session, cancellationToken).ConfigureAwait(false);
            currentSession = refreshed;
            await authSessionStore.SaveAsync(refreshed, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Authentication session refreshed for user {UserId}.", refreshed.UserId);
            await widgetRefreshService.RefreshAsync(refreshed.UserId, cancellationToken).ConfigureAwait(false);
            return refreshed;
        }
        finally
        {
            sessionLock.Release();
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var previousUserId = currentSession?.UserId;
            currentSession = null;
            await authSessionStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Authentication session cleared.");
            if (previousUserId is Guid userId)
            {
                await widgetRefreshService.ClearAsync(userId, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            sessionLock.Release();
        }
    }

    private async Task PersistSessionAsync(AuthSession session, CancellationToken cancellationToken)
    {
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            currentSession = session;
            await authSessionStore.SaveAsync(session, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Authentication session saved for user {UserId}.", session.UserId);
            await widgetRefreshService.RefreshAsync(session.UserId, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            sessionLock.Release();
        }
    }

    private async Task<AuthSession> RefreshSessionCoreAsync(AuthSession session, CancellationToken cancellationToken)
    {
        var response = await authRemoteClient.RefreshAsync(session.BaseUrl, session.RefreshToken, cancellationToken)
            .ConfigureAwait(false);

        return new AuthSession
        {
            Mode = AuthenticationMode.Remote,
            UserId = response.UserId,
            Email = session.Email,
            BaseUrl = session.BaseUrl,
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            AccessTokenExpiresAt = response.ExpiresAt,
            RestoredAt = DateTimeOffset.UtcNow
        };
    }

    private static AuthSession CreateSession(string baseUrl, string email, AuthTokenResult response)
    {
        return new AuthSession
        {
            Mode = AuthenticationMode.Remote,
            UserId = response.UserId,
            Email = NormalizeEmail(email),
            BaseUrl = NormalizeBaseUrl(baseUrl),
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            AccessTokenExpiresAt = response.ExpiresAt
        };
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL is required.", nameof(baseUrl));
        }

        return baseUrl.Trim().TrimEnd('/');
    }

    private static AuthSession CloneSession(AuthSession session, DateTimeOffset? restoredAt = null)
    {
        return new AuthSession
        {
            Mode = session.Mode,
            UserId = session.UserId,
            Email = session.Email,
            BaseUrl = session.BaseUrl,
            AccessToken = session.AccessToken,
            RefreshToken = session.RefreshToken,
            AccessTokenExpiresAt = session.AccessTokenExpiresAt,
            RestoredAt = restoredAt ?? session.RestoredAt
        };
    }
}
