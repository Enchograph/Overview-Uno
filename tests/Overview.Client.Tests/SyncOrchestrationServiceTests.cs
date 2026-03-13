using Overview.Client.Application.Auth;
using Overview.Client.Application.Items;
using Overview.Client.Application.Notifications;
using Overview.Client.Application.Settings;
using Overview.Client.Application.Sync;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Infrastructure.Api.Sync;
using Overview.Client.Infrastructure.Api.Sync.Contracts;
using Overview.Client.Infrastructure.Diagnostics;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Settings;

namespace Overview.Client.Tests;

public sealed class SyncOrchestrationServiceTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task StartAutoSyncAsync_ConvergesItemsAcrossDevicesWithoutManualSync()
    {
        var backend = new SharedSyncBackend();
        var deviceA = CreateHarness("device-a", backend);
        var deviceB = CreateHarness("device-b", backend);

        var created = await deviceA.ItemService.CreateAsync(UserId, new ItemUpsertRequest
        {
            Type = ItemType.Task,
            Title = "Review sync acceptance",
            PlannedStartAt = new DateTimeOffset(2026, 3, 13, 9, 0, 0, TimeSpan.Zero),
            PlannedEndAt = new DateTimeOffset(2026, 3, 13, 10, 0, 0, TimeSpan.Zero),
            DeadlineAt = new DateTimeOffset(2026, 3, 13, 18, 0, 0, TimeSpan.Zero)
        });

        await deviceA.SyncService.StartAutoSyncAsync(TimeSpan.FromMinutes(5));
        await deviceB.SyncService.StartAutoSyncAsync(TimeSpan.FromMinutes(5));

        var replicated = await deviceB.ItemRepository.GetAsync(UserId, created.Id);

        Assert.NotNull(replicated);
        Assert.Equal("Review sync acceptance", replicated!.Title);
        Assert.Equal(SyncExecutionTrigger.Automatic, deviceB.SyncService.CurrentStatus.LastTrigger);
        Assert.True(deviceB.SyncService.CurrentStatus.IsAutoSyncEnabled);
        Assert.Equal(SyncLifecycleState.Succeeded, deviceB.SyncService.CurrentStatus.State);

        await deviceA.SyncService.StopAutoSyncAsync();
        await deviceB.SyncService.StopAutoSyncAsync();
    }

    [Fact]
    public async Task StartAutoSyncAsync_ConvergesSettingsAcrossDevicesWithoutManualSync()
    {
        var backend = new SharedSyncBackend();
        var deviceA = CreateHarness("device-a", backend);
        var deviceB = CreateHarness("device-b", backend);

        await deviceA.UserSettingsService.SaveAsync(UserId, new UserSettingsUpdateRequest
        {
            Language = "en-US",
            ThemeMode = ThemeMode.Dark,
            ThemePreset = "slate",
            SyncServerBaseUrl = "https://sync.example.com",
            TimeZoneId = "UTC"
        });

        await deviceA.SyncService.StartAutoSyncAsync(TimeSpan.FromMinutes(5));
        await deviceB.SyncService.StartAutoSyncAsync(TimeSpan.FromMinutes(5));

        var replicated = await deviceB.UserSettingsRepository.GetAsync(UserId);

        Assert.NotNull(replicated);
        Assert.Equal("en-US", replicated!.Language);
        Assert.Equal(ThemeMode.Dark, replicated.ThemeMode);
        Assert.Equal("slate", replicated.ThemePreset);
        Assert.Equal(SyncExecutionTrigger.Automatic, deviceB.SyncService.CurrentStatus.LastTrigger);
        Assert.True(deviceB.SyncService.CurrentStatus.SettingsApplied);
        Assert.Equal(SyncLifecycleState.Succeeded, deviceB.SyncService.CurrentStatus.State);

        await deviceA.SyncService.StopAutoSyncAsync();
        await deviceB.SyncService.StopAutoSyncAsync();
    }

    [Fact]
    public async Task StartAutoSyncAsync_RefreshesNotificationsAfterApplyingRemoteChanges()
    {
        var backend = new SharedSyncBackend();
        var deviceA = CreateHarness("device-a", backend);
        var recordingNotifications = new RecordingNotificationRefreshService();
        var deviceB = CreateHarness("device-b", backend, recordingNotifications);

        await deviceA.ItemService.CreateAsync(UserId, new ItemUpsertRequest
        {
            Type = ItemType.Task,
            Title = "Review reminder sync",
            PlannedStartAt = new DateTimeOffset(2026, 3, 13, 9, 0, 0, TimeSpan.Zero),
            PlannedEndAt = new DateTimeOffset(2026, 3, 13, 10, 0, 0, TimeSpan.Zero),
            DeadlineAt = new DateTimeOffset(2026, 3, 13, 18, 0, 0, TimeSpan.Zero)
        });

        await deviceA.SyncService.StartAutoSyncAsync(TimeSpan.FromMinutes(5));
        await deviceB.SyncService.StartAutoSyncAsync(TimeSpan.FromMinutes(5));

        Assert.Equal([UserId], recordingNotifications.RefreshedUsers);

        await deviceA.SyncService.StopAutoSyncAsync();
        await deviceB.SyncService.StopAutoSyncAsync();
    }

    private static TestHarness CreateHarness(
        string deviceId,
        SharedSyncBackend backend,
        INotificationRefreshService? notificationRefreshService = null)
    {
        var session = new AuthSession
        {
            UserId = UserId,
            Email = "sync@example.com",
            BaseUrl = "https://sync.example.com",
            AccessToken = "token",
            RefreshToken = "refresh",
            AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        var itemRepository = new InMemoryItemRepository();
        var userSettingsRepository = new InMemoryUserSettingsRepository();
        var syncChangeRepository = new InMemorySyncChangeRepository();
        var syncService = new SyncOrchestrationService(
            new FakeAuthenticationService(session),
            itemRepository,
            userSettingsRepository,
            syncChangeRepository,
            new FakeSyncRemoteClient(UserId, backend),
            new InMemorySyncStateStore(),
            new FixedDeviceIdStore(deviceId),
            NullOverviewLoggerFactory.Instance,
            TimeProvider.System,
            notificationRefreshService);

        return new TestHarness(
            itemRepository,
            userSettingsRepository,
            new ItemService(itemRepository, syncChangeRepository, new FixedDeviceIdStore(deviceId), notificationRefreshService),
            new UserSettingsService(userSettingsRepository, syncChangeRepository, new FixedDeviceIdStore(deviceId), notificationRefreshService),
            syncService);
    }

    private sealed record TestHarness(
        InMemoryItemRepository ItemRepository,
        InMemoryUserSettingsRepository UserSettingsRepository,
        ItemService ItemService,
        UserSettingsService UserSettingsService,
        SyncOrchestrationService SyncService);

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public FakeAuthenticationService(AuthSession session)
        {
            CurrentSession = session;
        }

        public AuthSession? CurrentSession { get; private set; }

        public bool IsAuthenticated => CurrentSession is not null;

        public Task<VerificationCodeDispatchResult> SendVerificationCodeAsync(string baseUrl, string email, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession> RegisterAsync(string baseUrl, string email, string password, string verificationCode, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession> LoginAsync(string baseUrl, string email, string password, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession?> RestoreSessionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CurrentSession);
        }

        public Task<AuthSession> RefreshSessionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CurrentSession!);
        }

        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            CurrentSession = null;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedDeviceIdStore : IDeviceIdStore
    {
        private readonly string deviceId;

        public FixedDeviceIdStore(string deviceId)
        {
            this.deviceId = deviceId;
        }

        public Task<string> GetOrCreateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(deviceId);
        }
    }

    private sealed class RecordingNotificationRefreshService : INotificationRefreshService
    {
        public List<Guid> RefreshedUsers { get; } = [];

        public Task RefreshAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            RefreshedUsers.Add(userId);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryItemRepository : IItemRepository
    {
        private readonly Dictionary<Guid, Item> items = [];

        public Task<Item?> GetAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
        {
            items.TryGetValue(itemId, out var item);
            return Task.FromResult(item is not null && item.UserId == userId ? item : null);
        }

        public Task<IReadOnlyList<Item>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Item>>(items.Values.Where(item => item.UserId == userId).ToArray());
        }

        public Task UpsertAsync(Item item, CancellationToken cancellationToken = default)
        {
            items[item.Id] = item;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
        {
            items.Remove(itemId);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryUserSettingsRepository : IUserSettingsRepository
    {
        private readonly Dictionary<Guid, UserSettings> settingsByUser = [];

        public Task<UserSettings?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            settingsByUser.TryGetValue(userId, out var settings);
            return Task.FromResult(settings);
        }

        public Task UpsertAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            settingsByUser[settings.UserId] = settings;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemorySyncChangeRepository : ISyncChangeRepository
    {
        private readonly Dictionary<Guid, SyncChange> changes = [];

        public Task<IReadOnlyList<SyncChange>> ListPendingAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var pending = changes.Values
                .Where(change => change.UserId == userId && change.SyncedAt is null)
                .OrderBy(change => change.LastModifiedAt)
                .ToArray();
            return Task.FromResult<IReadOnlyList<SyncChange>>(pending);
        }

        public Task UpsertAsync(SyncChange change, CancellationToken cancellationToken = default)
        {
            changes[change.Id] = change;
            return Task.CompletedTask;
        }

        public Task MarkSyncedAsync(IEnumerable<Guid> changeIds, DateTimeOffset syncedAt, CancellationToken cancellationToken = default)
        {
            foreach (var changeId in changeIds)
            {
                if (!changes.TryGetValue(changeId, out var existing))
                {
                    continue;
                }

                changes[changeId] = new SyncChange
                {
                    Id = existing.Id,
                    UserId = existing.UserId,
                    DeviceId = existing.DeviceId,
                    EntityType = existing.EntityType,
                    ChangeType = existing.ChangeType,
                    EntityId = existing.EntityId,
                    ItemSnapshot = existing.ItemSnapshot,
                    SettingsSnapshot = existing.SettingsSnapshot,
                    CreatedAt = existing.CreatedAt,
                    LastModifiedAt = existing.LastModifiedAt,
                    SyncedAt = syncedAt
                };
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(IEnumerable<Guid> changeIds, CancellationToken cancellationToken = default)
        {
            foreach (var changeId in changeIds)
            {
                changes.Remove(changeId);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class InMemorySyncStateStore : ISyncStateStore
    {
        private readonly Dictionary<Guid, SyncCheckpoint> checkpoints = [];

        public Task<SyncCheckpoint?> LoadAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            checkpoints.TryGetValue(userId, out var checkpoint);
            return Task.FromResult(checkpoint);
        }

        public Task SaveAsync(SyncCheckpoint checkpoint, CancellationToken cancellationToken = default)
        {
            checkpoints[checkpoint.UserId] = checkpoint;
            return Task.CompletedTask;
        }

        public Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            checkpoints.Remove(userId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSyncRemoteClient : ISyncRemoteClient
    {
        private readonly Guid userId;
        private readonly SharedSyncBackend backend;

        public FakeSyncRemoteClient(Guid userId, SharedSyncBackend backend)
        {
            this.userId = userId;
            this.backend = backend;
        }

        public Task<SyncPullResponse> PullAsync(string baseUrl, string accessToken, DateTimeOffset? since = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(backend.Pull(userId, since));
        }

        public Task<SyncPushResponse> PushAsync(string baseUrl, string accessToken, SyncPushRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(backend.Push(userId, request));
        }
    }

    private sealed class SharedSyncBackend
    {
        private readonly object gate = new();
        private readonly Dictionary<Guid, Dictionary<Guid, Item>> itemsByUser = [];
        private readonly Dictionary<Guid, UserSettings> settingsByUser = [];
        private DateTimeOffset serverTime = new(2026, 3, 13, 0, 0, 0, TimeSpan.Zero);

        public SyncPushResponse Push(Guid userId, SyncPushRequest request)
        {
            lock (gate)
            {
                var conflicts = new List<SyncConflictContract>();
                var appliedCount = 0;
                var items = GetUserItems(userId);

                foreach (var change in request.ItemChanges)
                {
                    if (change.Item is null || change.EntityId is null)
                    {
                        continue;
                    }

                    var entityId = change.EntityId.Value;

                    if (items.TryGetValue(entityId, out var serverItem) &&
                        serverItem.LastModifiedAt > change.LastModifiedAt)
                    {
                        conflicts.Add(new SyncConflictContract
                        {
                            ChangeId = change.ChangeId,
                            EntityType = SyncEntityType.Item,
                            EntityId = entityId,
                            Reason = "Server item is newer.",
                            ServerLastModifiedAt = serverItem.LastModifiedAt,
                            ServerItem = serverItem
                        });
                        continue;
                    }

                    items[entityId] = change.Item;
                    appliedCount++;
                }

                if (request.SettingsChange is not null)
                {
                    var incoming = request.SettingsChange;
                    if (settingsByUser.TryGetValue(userId, out var serverSettings) &&
                        serverSettings.LastModifiedAt > incoming.LastModifiedAt)
                    {
                        conflicts.Add(new SyncConflictContract
                        {
                            ChangeId = incoming.ChangeId,
                            EntityType = SyncEntityType.UserSettings,
                            EntityId = incoming.Value.Id,
                            Reason = "Server settings are newer.",
                            ServerLastModifiedAt = serverSettings.LastModifiedAt,
                            ServerSettings = serverSettings
                        });
                    }
                    else
                    {
                        settingsByUser[userId] = incoming.Value;
                        appliedCount++;
                    }
                }

                serverTime = serverTime.AddSeconds(1);
                return new SyncPushResponse
                {
                    Accepted = conflicts.Count == 0,
                    ServerTime = serverTime,
                    AppliedChangeCount = appliedCount,
                    Conflicts = conflicts
                };
            }
        }

        public SyncPullResponse Pull(Guid userId, DateTimeOffset? since)
        {
            lock (gate)
            {
                var items = GetUserItems(userId)
                    .Values
                    .Where(item => since is null || item.LastModifiedAt > since.Value)
                    .Select(item => new SyncItemContract { Value = item })
                    .ToArray();

                settingsByUser.TryGetValue(userId, out var settings);
                var settingsContract = settings is not null && (since is null || settings.LastModifiedAt > since.Value)
                    ? new SyncSettingsContract { Value = settings }
                    : null;

                serverTime = serverTime.AddSeconds(1);
                return new SyncPullResponse
                {
                    ServerTime = serverTime,
                    Since = since,
                    Items = items,
                    Settings = settingsContract
                };
            }
        }

        private Dictionary<Guid, Item> GetUserItems(Guid userId)
        {
            if (!itemsByUser.TryGetValue(userId, out var items))
            {
                items = [];
                itemsByUser[userId] = items;
            }

            return items;
        }
    }
}
