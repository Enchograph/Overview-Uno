using Overview.Client.Application.Sync;

namespace Overview.Client.Infrastructure.Settings;

public interface ISyncStateStore
{
    Task<SyncCheckpoint?> LoadAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SaveAsync(SyncCheckpoint checkpoint, CancellationToken cancellationToken = default);

    Task ClearAsync(Guid userId, CancellationToken cancellationToken = default);
}
