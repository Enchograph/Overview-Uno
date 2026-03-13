namespace Overview.Client.Application.Sync;

public interface ISyncOrchestrationService
{
    SyncStatusSnapshot CurrentStatus { get; }

    event EventHandler<SyncStatusSnapshot>? StatusChanged;

    Task<SyncStatusSnapshot> InitializeAsync(CancellationToken cancellationToken = default);

    Task<SyncStatusSnapshot> SynchronizeNowAsync(CancellationToken cancellationToken = default);

    Task<SyncStatusSnapshot> StartAutoSyncAsync(
        TimeSpan? interval = null,
        CancellationToken cancellationToken = default);

    Task StopAutoSyncAsync(CancellationToken cancellationToken = default);
}
