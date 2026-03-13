using Overview.Client.Application.Sync;

namespace Overview.Client.Tests;

public sealed class SyncLifecycleCoordinatorTests
{
    [Fact]
    public async Task HandleShellLoadedAsync_StartsAutoSyncForAuthenticatedSession()
    {
        var syncService = new FakeSyncOrchestrationService
        {
            InitializeStatus = new SyncStatusSnapshot
            {
                IsAuthenticated = true
            }
        };

        var coordinator = new SyncLifecycleCoordinator(syncService);

        await coordinator.HandleShellLoadedAsync();

        Assert.Equal(1, syncService.InitializeCalls);
        Assert.Equal(1, syncService.StartAutoSyncCalls);
        Assert.Equal(0, syncService.SynchronizeNowCalls);
    }

    [Fact]
    public async Task HandleWindowActivatedAsync_StartsAutoSyncWhenNotRunning()
    {
        var syncService = new FakeSyncOrchestrationService
        {
            InitializeStatus = new SyncStatusSnapshot
            {
                IsAuthenticated = true,
                IsAutoSyncEnabled = false
            }
        };

        var coordinator = new SyncLifecycleCoordinator(syncService);

        await coordinator.HandleWindowActivatedAsync();

        Assert.Equal(1, syncService.InitializeCalls);
        Assert.Equal(1, syncService.StartAutoSyncCalls);
        Assert.Equal(0, syncService.SynchronizeNowCalls);
    }

    [Fact]
    public async Task HandleWindowActivatedAsync_PerformsImmediateSyncWhenAutoSyncAlreadyRunning()
    {
        var syncService = new FakeSyncOrchestrationService
        {
            InitializeStatus = new SyncStatusSnapshot
            {
                IsAuthenticated = true,
                IsAutoSyncEnabled = true
            }
        };

        var coordinator = new SyncLifecycleCoordinator(syncService);

        await coordinator.HandleWindowActivatedAsync();

        Assert.Equal(1, syncService.InitializeCalls);
        Assert.Equal(0, syncService.StartAutoSyncCalls);
        Assert.Equal(1, syncService.SynchronizeNowCalls);
    }

    [Fact]
    public async Task HandleShellUnloadedAsync_StopsAutoSync()
    {
        var syncService = new FakeSyncOrchestrationService();
        var coordinator = new SyncLifecycleCoordinator(syncService);

        await coordinator.HandleShellUnloadedAsync();

        Assert.Equal(1, syncService.StopAutoSyncCalls);
    }

    private sealed class FakeSyncOrchestrationService : ISyncOrchestrationService
    {
        public SyncStatusSnapshot CurrentStatus { get; private set; } = new();

        public event EventHandler<SyncStatusSnapshot>? StatusChanged;

        public SyncStatusSnapshot InitializeStatus { get; set; } = new();

        public int InitializeCalls { get; private set; }

        public int StartAutoSyncCalls { get; private set; }

        public int SynchronizeNowCalls { get; private set; }

        public int StopAutoSyncCalls { get; private set; }

        public Task<SyncStatusSnapshot> InitializeAsync(CancellationToken cancellationToken = default)
        {
            InitializeCalls++;
            CurrentStatus = InitializeStatus;
            StatusChanged?.Invoke(this, CurrentStatus);
            return Task.FromResult(CurrentStatus);
        }

        public Task<SyncStatusSnapshot> SynchronizeNowAsync(CancellationToken cancellationToken = default)
        {
            SynchronizeNowCalls++;
            return Task.FromResult(CurrentStatus);
        }

        public Task<SyncStatusSnapshot> StartAutoSyncAsync(TimeSpan? interval = null, CancellationToken cancellationToken = default)
        {
            StartAutoSyncCalls++;
            CurrentStatus = new SyncStatusSnapshot
            {
                IsAuthenticated = true,
                IsAutoSyncEnabled = true
            };
            return Task.FromResult(CurrentStatus);
        }

        public Task StopAutoSyncAsync(CancellationToken cancellationToken = default)
        {
            StopAutoSyncCalls++;
            CurrentStatus = new SyncStatusSnapshot
            {
                IsAuthenticated = CurrentStatus.IsAuthenticated,
                IsAutoSyncEnabled = false
            };
            return Task.CompletedTask;
        }
    }
}
