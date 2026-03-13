using Overview.Client.Application.Auth;

namespace Overview.Client.Infrastructure.Settings;

public interface IAuthSessionStore
{
    Task<AuthSession?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AuthSession session, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
