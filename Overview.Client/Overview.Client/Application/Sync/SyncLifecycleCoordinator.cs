namespace Overview.Client.Application.Sync;

public sealed class SyncLifecycleCoordinator : ISyncLifecycleCoordinator
{
    private readonly ISyncOrchestrationService syncOrchestrationService;

    public SyncLifecycleCoordinator(ISyncOrchestrationService syncOrchestrationService)
    {
        this.syncOrchestrationService = syncOrchestrationService;
    }

    public async Task HandleShellLoadedAsync(CancellationToken cancellationToken = default)
    {
        var status = await syncOrchestrationService.InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (!status.IsAuthenticated)
        {
            return;
        }

        await syncOrchestrationService.StartAutoSyncAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleWindowActivatedAsync(CancellationToken cancellationToken = default)
    {
        var status = await syncOrchestrationService.InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (!status.IsAuthenticated)
        {
            return;
        }

        if (!status.IsAutoSyncEnabled)
        {
            await syncOrchestrationService.StartAutoSyncAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }

        await syncOrchestrationService.SynchronizeNowAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task HandleShellUnloadedAsync(CancellationToken cancellationToken = default)
    {
        return syncOrchestrationService.StopAutoSyncAsync(cancellationToken);
    }
}
