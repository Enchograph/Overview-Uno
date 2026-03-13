using Overview.Client.Application.Items;
using Overview.Client.Application.Notifications;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;
using Overview.Client.Domain.ValueObjects;
using Overview.Client.Infrastructure.Notifications;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Settings;

namespace Overview.Client.Tests;

public sealed class NotificationRefreshServiceTests
{
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task RefreshAsync_SchedulesUpcomingReminders_AndCancelsStaleEntries()
    {
        var itemRepository = new InMemoryItemRepository();
        var settingsRepository = new InMemoryUserSettingsRepository(new UserSettings
        {
            UserId = UserId,
            NotificationEnabled = true,
            TimeZoneId = "UTC"
        });
        var scheduler = new RecordingNotificationScheduler();
        var stateStore = new InMemoryNotificationStateStore();
        var service = new NotificationRefreshService(
            itemRepository,
            settingsRepository,
            new ReminderRuleService(),
            scheduler,
            stateStore,
            new FixedTimeProvider(new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero)));

        var item = CreateTaskItem();
        await itemRepository.UpsertAsync(item);

        await service.RefreshAsync(UserId);

        Assert.Single(scheduler.Scheduled);
        var trackedIds = await stateStore.LoadAsync(UserId);
        Assert.Single(trackedIds);

        await itemRepository.UpsertAsync(new Item
        {
            Id = item.Id,
            UserId = item.UserId,
            Type = item.Type,
            Title = item.Title,
            ReminderConfig = item.ReminderConfig,
            RepeatRule = item.RepeatRule,
            TimeZoneId = item.TimeZoneId,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt.AddMinutes(1),
            LastModifiedAt = item.LastModifiedAt.AddMinutes(1),
            SourceDeviceId = item.SourceDeviceId,
            IsCompleted = true,
            PlannedStartAt = item.PlannedStartAt,
            PlannedEndAt = item.PlannedEndAt,
            DeadlineAt = item.DeadlineAt
        });

        await service.RefreshAsync(UserId);

        Assert.Single(scheduler.Canceled);
        Assert.Empty(await stateStore.LoadAsync(UserId));
    }

    [Fact]
    public async Task RefreshAsync_DisabledNotifications_CancelsTrackedEntries()
    {
        var itemRepository = new InMemoryItemRepository();
        var settingsRepository = new InMemoryUserSettingsRepository(new UserSettings
        {
            UserId = UserId,
            NotificationEnabled = true,
            TimeZoneId = "UTC"
        });
        var scheduler = new RecordingNotificationScheduler();
        var stateStore = new InMemoryNotificationStateStore();
        var service = new NotificationRefreshService(
            itemRepository,
            settingsRepository,
            new ReminderRuleService(),
            scheduler,
            stateStore,
            new FixedTimeProvider(new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero)));

        await itemRepository.UpsertAsync(CreateTaskItem());
        await service.RefreshAsync(UserId);

        settingsRepository.Current = new UserSettings
        {
            UserId = UserId,
            NotificationEnabled = false,
            TimeZoneId = "UTC"
        };

        await service.RefreshAsync(UserId);

        Assert.Single(scheduler.Canceled);
        Assert.Empty(await stateStore.LoadAsync(UserId));
    }

    [Fact]
    public async Task ItemService_CreateAsync_RefreshesNotifications()
    {
        var notificationRefreshService = new RecordingNotificationRefreshService();
        var service = new ItemService(
            new InMemoryItemRepository(),
            new InMemorySyncChangeRepository(),
            new FixedDeviceIdStore("device-a"),
            notificationRefreshService);

        await service.CreateAsync(UserId, new ItemUpsertRequest
        {
            Type = ItemType.Task,
            Title = "Task with reminder",
            PlannedStartAt = new DateTimeOffset(2026, 3, 14, 9, 0, 0, TimeSpan.Zero),
            PlannedEndAt = new DateTimeOffset(2026, 3, 14, 10, 0, 0, TimeSpan.Zero),
            DeadlineAt = new DateTimeOffset(2026, 3, 14, 18, 0, 0, TimeSpan.Zero),
            ReminderConfig = new ReminderConfig
            {
                IsEnabled = true,
                Triggers =
                [
                    new ReminderTrigger
                    {
                        Channel = ReminderChannel.Notification,
                        MinutesBeforeStart = 15
                    }
                ]
            }
        });

        Assert.Equal([UserId], notificationRefreshService.RefreshedUsers);
    }

    [Fact]
    public async Task UserSettingsService_SaveAsync_RefreshesNotifications()
    {
        var notificationRefreshService = new RecordingNotificationRefreshService();
        var service = new UserSettingsService(
            new InMemoryUserSettingsRepository(),
            new InMemorySyncChangeRepository(),
            new FixedDeviceIdStore("device-a"),
            notificationRefreshService);

        await service.SaveAsync(UserId, new UserSettingsUpdateRequest
        {
            Language = "en-US",
            SyncServerBaseUrl = "https://sync.example.com",
            TimeZoneId = "UTC"
        });

        Assert.Equal([UserId], notificationRefreshService.RefreshedUsers);
    }

    private static Item CreateTaskItem()
    {
        return new Item
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            UserId = UserId,
            Type = ItemType.Task,
            Title = "Morning review",
            ReminderConfig = new ReminderConfig
            {
                IsEnabled = true,
                Triggers =
                [
                    new ReminderTrigger
                    {
                        Channel = ReminderChannel.Notification,
                        MinutesBeforeStart = 15
                    }
                ]
            },
            RepeatRule = new RepeatRule(),
            TimeZoneId = "UTC",
            CreatedAt = new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero),
            LastModifiedAt = new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero),
            SourceDeviceId = "device-a",
            PlannedStartAt = new DateTimeOffset(2026, 3, 14, 9, 0, 0, TimeSpan.Zero),
            PlannedEndAt = new DateTimeOffset(2026, 3, 14, 10, 0, 0, TimeSpan.Zero),
            DeadlineAt = new DateTimeOffset(2026, 3, 14, 18, 0, 0, TimeSpan.Zero)
        };
    }

    private sealed class RecordingNotificationScheduler : INotificationScheduler
    {
        public List<NotificationScheduleRequest> Scheduled { get; } = [];

        public List<string> Canceled { get; } = [];

        public Task ScheduleAsync(IReadOnlyCollection<NotificationScheduleRequest> requests, CancellationToken cancellationToken)
        {
            Scheduled.AddRange(requests);
            return Task.CompletedTask;
        }

        public Task CancelAsync(IReadOnlyCollection<string> notificationIds, CancellationToken cancellationToken)
        {
            Canceled.AddRange(notificationIds);
            return Task.CompletedTask;
        }

        public Task CancelAllAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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
        public InMemoryUserSettingsRepository(UserSettings? current = null)
        {
            Current = current;
        }

        public UserSettings? Current { get; set; }

        public Task<UserSettings?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Current is not null && Current.UserId == userId ? Current : null);
        }

        public Task UpsertAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            Current = settings;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemorySyncChangeRepository : ISyncChangeRepository
    {
        private readonly Dictionary<Guid, SyncChange> changes = [];

        public Task<IReadOnlyList<SyncChange>> ListPendingAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SyncChange>>(changes.Values.Where(change => change.UserId == userId).ToArray());
        }

        public Task UpsertAsync(SyncChange change, CancellationToken cancellationToken = default)
        {
            changes[change.Id] = change;
            return Task.CompletedTask;
        }

        public Task MarkSyncedAsync(IEnumerable<Guid> changeIds, DateTimeOffset syncedAt, CancellationToken cancellationToken = default)
        {
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

    private sealed class FixedDeviceIdStore(string deviceId) : IDeviceIdStore
    {
        public Task<string> GetOrCreateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(deviceId);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return now;
        }
    }
}
