using System.Net;
using Overview.Client.Application.Auth;
using Overview.Client.Application.Notifications;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Infrastructure.Api.Sync;
using Overview.Client.Infrastructure.Api.Sync.Contracts;
using Overview.Client.Infrastructure.Diagnostics;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Settings;

namespace Overview.Client.Application.Sync;

public sealed class SyncOrchestrationService : ISyncOrchestrationService
{
    private static readonly TimeSpan DefaultAutoSyncInterval = TimeSpan.FromMinutes(2);

    private readonly IAuthenticationService authenticationService;
    private readonly IItemRepository itemRepository;
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly ISyncChangeRepository syncChangeRepository;
    private readonly ISyncRemoteClient syncRemoteClient;
    private readonly ISyncStateStore syncStateStore;
    private readonly IDeviceIdStore deviceIdStore;
    private readonly IOverviewLogger logger;
    private readonly TimeProvider timeProvider;
    private readonly INotificationRefreshService notificationRefreshService;
    private readonly SemaphoreSlim syncLock = new(1, 1);

    private readonly object autoSyncGate = new();
    private CancellationTokenSource? autoSyncCancellationTokenSource;
    private Task? autoSyncTask;
    private SyncCheckpoint? currentCheckpoint;
    private bool isAutoSyncEnabled;

    public SyncOrchestrationService(
        IAuthenticationService authenticationService,
        IItemRepository itemRepository,
        IUserSettingsRepository userSettingsRepository,
        ISyncChangeRepository syncChangeRepository,
        ISyncRemoteClient syncRemoteClient,
        ISyncStateStore syncStateStore,
        IDeviceIdStore deviceIdStore,
        IOverviewLoggerFactory loggerFactory,
        TimeProvider timeProvider,
        INotificationRefreshService? notificationRefreshService = null)
    {
        this.authenticationService = authenticationService;
        this.itemRepository = itemRepository;
        this.userSettingsRepository = userSettingsRepository;
        this.syncChangeRepository = syncChangeRepository;
        this.syncRemoteClient = syncRemoteClient;
        this.syncStateStore = syncStateStore;
        this.deviceIdStore = deviceIdStore;
        logger = loggerFactory.CreateLogger<SyncOrchestrationService>();
        this.timeProvider = timeProvider;
        this.notificationRefreshService = notificationRefreshService ?? NoOpNotificationRefreshService.Instance;
        CurrentStatus = new SyncStatusSnapshot();
    }

    public SyncStatusSnapshot CurrentStatus { get; private set; }

    public event EventHandler<SyncStatusSnapshot>? StatusChanged;

    public async Task<SyncStatusSnapshot> InitializeAsync(CancellationToken cancellationToken = default)
    {
        var session = authenticationService.CurrentSession
            ?? await authenticationService.RestoreSessionAsync(cancellationToken).ConfigureAwait(false);

        SyncCheckpoint? checkpoint = null;
        int pendingChangeCount = 0;

        if (session is not null)
        {
            checkpoint = await syncStateStore.LoadAsync(session.UserId, cancellationToken).ConfigureAwait(false);
            pendingChangeCount = (await syncChangeRepository.ListPendingAsync(session.UserId, cancellationToken).ConfigureAwait(false)).Count;
        }

        currentCheckpoint = checkpoint;
        UpdateStatus(new SyncStatusSnapshot
        {
            State = session is null ? SyncLifecycleState.RequiresAuthentication : SyncLifecycleState.Idle,
            LastTrigger = checkpoint?.LastTrigger,
            LastAttemptedAt = checkpoint?.LastAttemptedAt,
            LastSuccessfulAt = checkpoint?.LastSuccessfulAt,
            LastKnownServerTime = checkpoint?.LastKnownServerTime,
            PendingChangeCount = pendingChangeCount,
            ConflictCount = 0,
            AppliedChangeCount = 0,
            PulledItemCount = 0,
            SettingsApplied = false,
            ConsecutiveFailureCount = checkpoint?.ConsecutiveFailureCount ?? 0,
            IsAutoSyncEnabled = isAutoSyncEnabled,
            IsAuthenticated = session is not null,
            LastError = checkpoint?.LastError
        });

        return CurrentStatus;
    }

    public Task<SyncStatusSnapshot> SynchronizeNowAsync(CancellationToken cancellationToken = default)
    {
        return SynchronizeCoreAsync(SyncExecutionTrigger.Manual, cancellationToken);
    }

    public async Task<SyncStatusSnapshot> StartAutoSyncAsync(
        TimeSpan? interval = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveInterval = interval ?? DefaultAutoSyncInterval;
        if (effectiveInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Auto sync interval must be greater than zero.");
        }

        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        lock (autoSyncGate)
        {
            if (autoSyncTask is null || autoSyncTask.IsCompleted)
            {
                autoSyncCancellationTokenSource = new CancellationTokenSource();
                autoSyncTask = RunAutoSyncLoopAsync(effectiveInterval, autoSyncCancellationTokenSource.Token);
            }

            isAutoSyncEnabled = true;
        }

        UpdateStatus(CloneCurrentStatus(
            state: CurrentStatus.State,
            isAutoSyncEnabled: true));

        return await SynchronizeCoreAsync(SyncExecutionTrigger.Automatic, cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAutoSyncAsync(CancellationToken cancellationToken = default)
    {
        Task? taskToAwait = null;

        lock (autoSyncGate)
        {
            isAutoSyncEnabled = false;
            autoSyncCancellationTokenSource?.Cancel();
            taskToAwait = autoSyncTask;
            autoSyncCancellationTokenSource = null;
            autoSyncTask = null;
        }

        UpdateStatus(CloneCurrentStatus(isAutoSyncEnabled: false));

        if (taskToAwait is null)
        {
            return;
        }

        try
        {
            await taskToAwait.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RunAutoSyncLoopAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await SynchronizeCoreAsync(SyncExecutionTrigger.Automatic, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task<SyncStatusSnapshot> SynchronizeCoreAsync(
        SyncExecutionTrigger trigger,
        CancellationToken cancellationToken)
    {
        await syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var session = authenticationService.CurrentSession
                ?? await authenticationService.RestoreSessionAsync(cancellationToken).ConfigureAwait(false);

            if (session is null)
            {
                UpdateStatus(CloneCurrentStatus(
                    state: SyncLifecycleState.RequiresAuthentication,
                    lastTrigger: trigger,
                    lastAttemptedAt: timeProvider.GetUtcNow(),
                    isAuthenticated: false,
                    lastError: "Authentication session is unavailable."));

                return CurrentStatus;
            }

            currentCheckpoint ??= await syncStateStore.LoadAsync(session.UserId, cancellationToken).ConfigureAwait(false)
                ?? new SyncCheckpoint { UserId = session.UserId };

            var pendingChanges = await syncChangeRepository.ListPendingAsync(session.UserId, cancellationToken).ConfigureAwait(false);
            var attemptTime = timeProvider.GetUtcNow();

            UpdateStatus(new SyncStatusSnapshot
            {
                State = SyncLifecycleState.Running,
                LastTrigger = trigger,
                LastAttemptedAt = attemptTime,
                LastSuccessfulAt = currentCheckpoint.LastSuccessfulAt,
                LastKnownServerTime = currentCheckpoint.LastKnownServerTime,
                PendingChangeCount = pendingChanges.Count,
                AppliedChangeCount = 0,
                PulledItemCount = 0,
                SettingsApplied = false,
                ConflictCount = 0,
                ConsecutiveFailureCount = currentCheckpoint.ConsecutiveFailureCount,
                IsAutoSyncEnabled = isAutoSyncEnabled,
                IsAuthenticated = true,
                LastError = null
            });

            var accessToken = await EnsureAccessTokenAsync(session, cancellationToken).ConfigureAwait(false);
            var settings = await userSettingsRepository.GetAsync(session.UserId, cancellationToken).ConfigureAwait(false);
            var baseUrl = ResolveBaseUrl(session, settings);
            var deviceId = await deviceIdStore.GetOrCreateAsync(cancellationToken).ConfigureAwait(false);

            var itemChanges = pendingChanges
                .Where(change => change.EntityType == SyncEntityType.Item)
                .OrderBy(change => change.LastModifiedAt)
                .Select(change => new SyncItemChangeRequest
                {
                    ChangeId = change.Id,
                    ChangeType = change.ChangeType,
                    EntityId = change.EntityId,
                    Item = change.ItemSnapshot,
                    CreatedAt = change.CreatedAt,
                    LastModifiedAt = change.LastModifiedAt
                })
                .ToArray();

            var latestSettingsChange = pendingChanges
                .Where(change => change.EntityType == SyncEntityType.UserSettings && change.SettingsSnapshot is not null)
                .OrderByDescending(change => change.LastModifiedAt)
                .FirstOrDefault();

            var pushRequest = new SyncPushRequest
            {
                DeviceId = deviceId,
                LastKnownServerTime = currentCheckpoint.LastKnownServerTime,
                ItemChanges = itemChanges,
                SettingsChange = latestSettingsChange is null
                    ? null
                    : new SyncSettingsChangeRequest
                    {
                        ChangeId = latestSettingsChange.Id,
                        Value = latestSettingsChange.SettingsSnapshot!,
                        CreatedAt = latestSettingsChange.CreatedAt,
                        LastModifiedAt = latestSettingsChange.LastModifiedAt
                    }
            };

            var pushResponse = await PushWithRefreshAsync(baseUrl, accessToken, pushRequest, cancellationToken).ConfigureAwait(false);

            var pendingById = pendingChanges.ToDictionary(change => change.Id);
            var conflictIds = new HashSet<Guid>(pushResponse.Conflicts.Select(conflict => conflict.ChangeId));

            var acceptedChangeIds = pendingChanges
                .Where(change =>
                    !conflictIds.Contains(change.Id) &&
                    (change.EntityType == SyncEntityType.Item ||
                     (latestSettingsChange is not null && change.Id == latestSettingsChange.Id)))
                .Select(change => change.Id)
                .ToArray();

            if (acceptedChangeIds.Length > 0)
            {
                await syncChangeRepository.MarkSyncedAsync(acceptedChangeIds, pushResponse.ServerTime, cancellationToken).ConfigureAwait(false);
            }

            if (latestSettingsChange is not null)
            {
                var supersededSettingsChanges = pendingChanges
                    .Where(change =>
                        change.EntityType == SyncEntityType.UserSettings &&
                        change.Id != latestSettingsChange.Id &&
                        change.LastModifiedAt <= latestSettingsChange.LastModifiedAt)
                    .Select(change => change.Id)
                    .ToArray();

                if (supersededSettingsChanges.Length > 0)
                {
                    await syncChangeRepository.DeleteAsync(supersededSettingsChanges, cancellationToken).ConfigureAwait(false);
                }
            }

            await ApplyConflictSnapshotsAsync(session.UserId, pushResponse.Conflicts, pendingChanges, cancellationToken).ConfigureAwait(false);

            var pullResponse = await PullWithRefreshAsync(
                baseUrl,
                accessToken,
                currentCheckpoint.LastKnownServerTime,
                cancellationToken).ConfigureAwait(false);

            var currentPendingChanges = await syncChangeRepository.ListPendingAsync(session.UserId, cancellationToken).ConfigureAwait(false);
            var pulledItemCount = await ApplyPulledItemsAsync(session.UserId, pullResponse.Items, currentPendingChanges, cancellationToken).ConfigureAwait(false);
            var settingsApplied = await ApplyPulledSettingsAsync(session.UserId, pullResponse.Settings, currentPendingChanges, cancellationToken).ConfigureAwait(false);
            await notificationRefreshService.RefreshAsync(session.UserId, cancellationToken).ConfigureAwait(false);

            var checkpoint = new SyncCheckpoint
            {
                UserId = session.UserId,
                LastKnownServerTime = Max(pushResponse.ServerTime, pullResponse.ServerTime),
                LastAttemptedAt = attemptTime,
                LastSuccessfulAt = timeProvider.GetUtcNow(),
                LastTrigger = trigger,
                LastError = null,
                ConsecutiveFailureCount = 0
            };

            currentCheckpoint = checkpoint;
            await syncStateStore.SaveAsync(checkpoint, cancellationToken).ConfigureAwait(false);

            var remainingPendingChanges = await syncChangeRepository.ListPendingAsync(session.UserId, cancellationToken).ConfigureAwait(false);
            UpdateStatus(new SyncStatusSnapshot
            {
                State = SyncLifecycleState.Succeeded,
                LastTrigger = trigger,
                LastAttemptedAt = checkpoint.LastAttemptedAt,
                LastSuccessfulAt = checkpoint.LastSuccessfulAt,
                LastKnownServerTime = checkpoint.LastKnownServerTime,
                PendingChangeCount = remainingPendingChanges.Count,
                AppliedChangeCount = pushResponse.AppliedChangeCount,
                PulledItemCount = pulledItemCount,
                SettingsApplied = settingsApplied,
                ConflictCount = pushResponse.Conflicts.Count,
                ConsecutiveFailureCount = 0,
                IsAutoSyncEnabled = isAutoSyncEnabled,
                IsAuthenticated = true,
                LastError = null
            });

            return CurrentStatus;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Sync execution failed.");

            if (await HandleAuthenticationFailureAsync(ex, cancellationToken).ConfigureAwait(false))
            {
                return CurrentStatus;
            }

            var failureCount = (currentCheckpoint?.ConsecutiveFailureCount ?? 0) + 1;
            var failureCheckpoint = new SyncCheckpoint
            {
                UserId = currentCheckpoint?.UserId ?? Guid.Empty,
                LastKnownServerTime = currentCheckpoint?.LastKnownServerTime,
                LastAttemptedAt = timeProvider.GetUtcNow(),
                LastSuccessfulAt = currentCheckpoint?.LastSuccessfulAt,
                LastTrigger = trigger,
                LastError = ex.Message,
                ConsecutiveFailureCount = failureCount
            };

            if (failureCheckpoint.UserId != Guid.Empty)
            {
                currentCheckpoint = failureCheckpoint;
                await syncStateStore.SaveAsync(failureCheckpoint, cancellationToken).ConfigureAwait(false);
            }

            UpdateStatus(CloneCurrentStatus(
                state: SyncLifecycleState.Failed,
                lastTrigger: trigger,
                lastAttemptedAt: failureCheckpoint.LastAttemptedAt,
                consecutiveFailureCount: failureCount,
                lastError: ex.Message));

            return CurrentStatus;
        }
        finally
        {
            syncLock.Release();
        }
    }

    private async Task<string> EnsureAccessTokenAsync(AuthSession session, CancellationToken cancellationToken)
    {
        if (session.AccessTokenExpiresAt > timeProvider.GetUtcNow().AddMinutes(1))
        {
            return session.AccessToken;
        }

        var refreshedSession = await authenticationService.RefreshSessionAsync(cancellationToken).ConfigureAwait(false);
        return refreshedSession.AccessToken;
    }

    private async Task<SyncPushResponse> PushWithRefreshAsync(
        string baseUrl,
        string accessToken,
        SyncPushRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await syncRemoteClient.PushAsync(baseUrl, accessToken, request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (IsUnauthorized(ex))
        {
            var refreshedSession = await authenticationService.RefreshSessionAsync(cancellationToken).ConfigureAwait(false);
            return await syncRemoteClient.PushAsync(
                baseUrl,
                refreshedSession.AccessToken,
                request,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<SyncPullResponse> PullWithRefreshAsync(
        string baseUrl,
        string accessToken,
        DateTimeOffset? since,
        CancellationToken cancellationToken)
    {
        try
        {
            return await syncRemoteClient.PullAsync(baseUrl, accessToken, since, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (IsUnauthorized(ex))
        {
            var refreshedSession = await authenticationService.RefreshSessionAsync(cancellationToken).ConfigureAwait(false);
            return await syncRemoteClient.PullAsync(
                baseUrl,
                refreshedSession.AccessToken,
                since,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ApplyConflictSnapshotsAsync(
        Guid userId,
        IReadOnlyList<SyncConflictContract> conflicts,
        IReadOnlyList<SyncChange> originalPendingChanges,
        CancellationToken cancellationToken)
    {
        if (conflicts.Count == 0)
        {
            return;
        }

        var deleteIds = new HashSet<Guid>(conflicts.Select(conflict => conflict.ChangeId));

        foreach (var conflict in conflicts)
        {
            switch (conflict.EntityType)
            {
                case SyncEntityType.Item when conflict.ServerItem is not null:
                    await itemRepository.UpsertAsync(conflict.ServerItem, cancellationToken).ConfigureAwait(false);
                    AddSupersededPendingChanges(deleteIds, originalPendingChanges, SyncEntityType.Item, conflict.ServerItem.Id, conflict.ServerLastModifiedAt);
                    break;

                case SyncEntityType.UserSettings when conflict.ServerSettings is not null:
                    await userSettingsRepository.UpsertAsync(conflict.ServerSettings, cancellationToken).ConfigureAwait(false);
                    AddSupersededPendingChanges(deleteIds, originalPendingChanges, SyncEntityType.UserSettings, conflict.ServerSettings.Id, conflict.ServerLastModifiedAt);
                    break;
            }
        }

        if (deleteIds.Count > 0)
        {
            await syncChangeRepository.DeleteAsync(deleteIds, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<int> ApplyPulledItemsAsync(
        Guid userId,
        IReadOnlyList<SyncItemContract> pulledItems,
        IReadOnlyList<SyncChange> pendingChanges,
        CancellationToken cancellationToken)
    {
        var appliedCount = 0;
        var discardedChangeIds = new HashSet<Guid>();

        foreach (var contract in pulledItems)
        {
            var remoteItem = contract.Value;
            var localItem = await itemRepository.GetAsync(userId, remoteItem.Id, cancellationToken).ConfigureAwait(false);
            if (localItem is not null && localItem.LastModifiedAt > remoteItem.LastModifiedAt)
            {
                continue;
            }

            await itemRepository.UpsertAsync(remoteItem, cancellationToken).ConfigureAwait(false);
            AddSupersededPendingChanges(discardedChangeIds, pendingChanges, SyncEntityType.Item, remoteItem.Id, remoteItem.LastModifiedAt);
            appliedCount++;
        }

        if (discardedChangeIds.Count > 0)
        {
            await syncChangeRepository.DeleteAsync(discardedChangeIds, cancellationToken).ConfigureAwait(false);
        }

        return appliedCount;
    }

    private async Task<bool> ApplyPulledSettingsAsync(
        Guid userId,
        SyncSettingsContract? pulledSettings,
        IReadOnlyList<SyncChange> pendingChanges,
        CancellationToken cancellationToken)
    {
        if (pulledSettings is null)
        {
            return false;
        }

        var remoteSettings = pulledSettings.Value;
        var localSettings = await userSettingsRepository.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        if (localSettings is not null && localSettings.LastModifiedAt > remoteSettings.LastModifiedAt)
        {
            return false;
        }

        await userSettingsRepository.UpsertAsync(remoteSettings, cancellationToken).ConfigureAwait(false);

        var discardedChangeIds = new HashSet<Guid>();
        AddSupersededPendingChanges(
            discardedChangeIds,
            pendingChanges,
            SyncEntityType.UserSettings,
            remoteSettings.Id,
            remoteSettings.LastModifiedAt);

        if (discardedChangeIds.Count > 0)
        {
            await syncChangeRepository.DeleteAsync(discardedChangeIds, cancellationToken).ConfigureAwait(false);
        }

        return true;
    }

    private async Task<bool> HandleAuthenticationFailureAsync(Exception exception, CancellationToken cancellationToken)
    {
        if (exception is HttpRequestException httpRequestException && IsUnauthorized(httpRequestException))
        {
            await authenticationService.LogoutAsync(cancellationToken).ConfigureAwait(false);

            if (currentCheckpoint?.UserId is Guid userId && userId != Guid.Empty)
            {
                currentCheckpoint = new SyncCheckpoint
                {
                    UserId = userId,
                    LastKnownServerTime = currentCheckpoint.LastKnownServerTime,
                    LastAttemptedAt = timeProvider.GetUtcNow(),
                    LastSuccessfulAt = currentCheckpoint.LastSuccessfulAt,
                    LastTrigger = CurrentStatus.LastTrigger,
                    LastError = "Authentication is no longer valid.",
                    ConsecutiveFailureCount = (currentCheckpoint.ConsecutiveFailureCount + 1)
                };
                await syncStateStore.SaveAsync(currentCheckpoint, cancellationToken).ConfigureAwait(false);
            }

            UpdateStatus(CloneCurrentStatus(
                state: SyncLifecycleState.RequiresAuthentication,
                isAuthenticated: false,
                lastAttemptedAt: timeProvider.GetUtcNow(),
                lastError: "Authentication is no longer valid.",
                consecutiveFailureCount: (currentCheckpoint?.ConsecutiveFailureCount ?? 0)));

            return true;
        }

        return false;
    }

    private static void AddSupersededPendingChanges(
        HashSet<Guid> deleteIds,
        IReadOnlyList<SyncChange> pendingChanges,
        SyncEntityType entityType,
        Guid entityId,
        DateTimeOffset serverLastModifiedAt)
    {
        foreach (var pendingChange in pendingChanges)
        {
            if (pendingChange.EntityType != entityType ||
                pendingChange.EntityId != entityId ||
                pendingChange.LastModifiedAt > serverLastModifiedAt)
            {
                continue;
            }

            deleteIds.Add(pendingChange.Id);
        }
    }

    private static bool IsUnauthorized(HttpRequestException exception)
    {
        return exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
    }

    private static string ResolveBaseUrl(AuthSession session, UserSettings? settings)
    {
        var configuredUrl = string.IsNullOrWhiteSpace(settings?.SyncServerBaseUrl)
            ? null
            : settings.SyncServerBaseUrl.Trim().TrimEnd('/');

        return configuredUrl ?? session.BaseUrl;
    }

    private static DateTimeOffset Max(DateTimeOffset left, DateTimeOffset right)
    {
        return left >= right ? left : right;
    }

    private SyncStatusSnapshot CloneCurrentStatus(
        SyncLifecycleState? state = null,
        SyncExecutionTrigger? lastTrigger = null,
        DateTimeOffset? lastAttemptedAt = null,
        DateTimeOffset? lastSuccessfulAt = null,
        DateTimeOffset? lastKnownServerTime = null,
        int? pendingChangeCount = null,
        int? appliedChangeCount = null,
        int? pulledItemCount = null,
        bool? settingsApplied = null,
        int? conflictCount = null,
        int? consecutiveFailureCount = null,
        bool? isAutoSyncEnabled = null,
        bool? isAuthenticated = null,
        string? lastError = null)
    {
        return new SyncStatusSnapshot
        {
            State = state ?? CurrentStatus.State,
            LastTrigger = lastTrigger ?? CurrentStatus.LastTrigger,
            LastAttemptedAt = lastAttemptedAt ?? CurrentStatus.LastAttemptedAt,
            LastSuccessfulAt = lastSuccessfulAt ?? CurrentStatus.LastSuccessfulAt,
            LastKnownServerTime = lastKnownServerTime ?? CurrentStatus.LastKnownServerTime,
            PendingChangeCount = pendingChangeCount ?? CurrentStatus.PendingChangeCount,
            AppliedChangeCount = appliedChangeCount ?? CurrentStatus.AppliedChangeCount,
            PulledItemCount = pulledItemCount ?? CurrentStatus.PulledItemCount,
            SettingsApplied = settingsApplied ?? CurrentStatus.SettingsApplied,
            ConflictCount = conflictCount ?? CurrentStatus.ConflictCount,
            ConsecutiveFailureCount = consecutiveFailureCount ?? CurrentStatus.ConsecutiveFailureCount,
            IsAutoSyncEnabled = isAutoSyncEnabled ?? CurrentStatus.IsAutoSyncEnabled,
            IsAuthenticated = isAuthenticated ?? CurrentStatus.IsAuthenticated,
            LastError = lastError
        };
    }

    private void UpdateStatus(SyncStatusSnapshot nextStatus)
    {
        CurrentStatus = nextStatus;
        StatusChanged?.Invoke(this, nextStatus);
    }
}
