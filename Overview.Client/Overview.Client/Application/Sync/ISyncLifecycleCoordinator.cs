namespace Overview.Client.Application.Sync;

public interface ISyncLifecycleCoordinator
{
    Task HandleShellLoadedAsync(CancellationToken cancellationToken = default);

    Task HandleWindowActivatedAsync(CancellationToken cancellationToken = default);

    Task HandleShellUnloadedAsync(CancellationToken cancellationToken = default);
}
