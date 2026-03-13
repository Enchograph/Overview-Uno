using Overview.Client.Application.Items;
using Overview.Client.Application.Settings;
using Overview.Client.Application.Widgets;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;
using Overview.Client.Domain.ValueObjects;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Settings;
using Overview.Client.Infrastructure.Widgets;

namespace Overview.Client.Tests;

public sealed class WidgetRefreshServiceTests
{
    private static readonly Guid UserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task RefreshAsync_PersistsSnapshotsForAllEnabledWidgets()
    {
        var itemRepository = new FakeItemRepository(
        [
            new Item
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                Type = ItemType.Task,
                Title = "Review roadmap",
                IsImportant = true,
                CreatedAt = new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero),
                SourceDeviceId = "device-a",
                TimeZoneId = "UTC",
                PlannedStartAt = new DateTimeOffset(2026, 3, 13, 9, 0, 0, TimeSpan.Zero),
                PlannedEndAt = new DateTimeOffset(2026, 3, 13, 10, 0, 0, TimeSpan.Zero),
                DeadlineAt = new DateTimeOffset(2026, 3, 13, 18, 0, 0, TimeSpan.Zero)
            }
        ]);
        var settingsRepository = new FakeUserSettingsRepository(new UserSettings
        {
            UserId = UserId,
            Language = "en-US",
            TimeZoneId = "UTC"
        });
        var messageRepository = new FakeAiChatMessageRepository(
        [
            new AiChatMessage
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                Role = AiChatRole.Assistant,
                Message = "You have one important task today.",
                OccurredOn = new DateOnly(2026, 3, 13),
                CreatedAt = new DateTimeOffset(2026, 3, 13, 8, 30, 0, TimeSpan.Zero)
            }
        ]);
        var snapshotStore = new InMemoryWidgetSnapshotStore();
        var renderer = new RecordingWidgetRenderer();
        var service = new WidgetRefreshService(
            itemRepository,
            settingsRepository,
            messageRepository,
            new TimeRuleService(),
            snapshotStore,
            renderer,
            new FixedTimeProvider(new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero)));

        await service.RefreshAsync(UserId);

        Assert.NotNull(await snapshotStore.GetAsync(WidgetKind.Home, default));
        Assert.NotNull(await snapshotStore.GetAsync(WidgetKind.List, default));
        Assert.NotNull(await snapshotStore.GetAsync(WidgetKind.AiShortcut, default));
        Assert.NotNull(await snapshotStore.GetAsync(WidgetKind.QuickAdd, default));
        Assert.Equal(
            [WidgetKind.Home, WidgetKind.List, WidgetKind.AiShortcut, WidgetKind.QuickAdd],
            renderer.RenderedKinds);
    }

    [Fact]
    public async Task RefreshAsync_DisabledListWidget_RemovesSnapshotAndClearsRenderer()
    {
        var snapshotStore = new InMemoryWidgetSnapshotStore();
        await snapshotStore.SaveAsync(new WidgetSnapshot
        {
            Kind = WidgetKind.List,
            Title = "Existing list widget",
            GeneratedAt = DateTimeOffset.UtcNow
        }, default);
        var renderer = new RecordingWidgetRenderer();
        var service = new WidgetRefreshService(
            new FakeItemRepository([]),
            new FakeUserSettingsRepository(new UserSettings
            {
                UserId = UserId,
                TimeZoneId = "UTC",
                WidgetPreferences = new WidgetPreferences
                {
                    EnableListWidget = false
                }
            }),
            new FakeAiChatMessageRepository([]),
            new TimeRuleService(),
            snapshotStore,
            renderer,
            TimeProvider.System);

        await service.RefreshAsync(UserId);

        Assert.Null(await snapshotStore.GetAsync(WidgetKind.List, default));
        Assert.Contains(WidgetKind.List, renderer.ClearedKinds);
    }

    [Fact]
    public async Task ItemService_CreateAsync_RefreshesWidgets()
    {
        var widgetRefreshService = new RecordingWidgetRefreshService();
        var service = new ItemService(
            new FakeItemRepository([]),
            new InMemorySyncChangeRepository(),
            new FixedDeviceIdStore("device-a"),
            notificationRefreshService: null,
            widgetRefreshService: widgetRefreshService);

        await service.CreateAsync(UserId, new ItemUpsertRequest
        {
            Type = ItemType.Task,
            Title = "Widget refresh task",
            PlannedStartAt = new DateTimeOffset(2026, 3, 14, 9, 0, 0, TimeSpan.Zero),
            PlannedEndAt = new DateTimeOffset(2026, 3, 14, 10, 0, 0, TimeSpan.Zero),
            DeadlineAt = new DateTimeOffset(2026, 3, 14, 18, 0, 0, TimeSpan.Zero)
        });

        Assert.Equal([UserId], widgetRefreshService.RefreshedUsers);
    }

    [Fact]
    public async Task UserSettingsService_SaveAsync_RefreshesWidgets()
    {
        var widgetRefreshService = new RecordingWidgetRefreshService();
        var service = new UserSettingsService(
            new FakeUserSettingsRepository(),
            new InMemorySyncChangeRepository(),
            new FixedDeviceIdStore("device-a"),
            notificationRefreshService: null,
            widgetRefreshService: widgetRefreshService);

        await service.SaveAsync(UserId, new UserSettingsUpdateRequest
        {
            Language = "en-US",
            SyncServerBaseUrl = "https://sync.example.com",
            TimeZoneId = "UTC"
        });

        Assert.Equal([UserId], widgetRefreshService.RefreshedUsers);
    }

    private sealed class RecordingWidgetRenderer : IWidgetRenderer
    {
        public List<WidgetKind> RenderedKinds { get; } = [];

        public List<WidgetKind> ClearedKinds { get; } = [];

        public Task RenderAsync(WidgetSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            RenderedKinds.Add(snapshot.Kind);
            return Task.CompletedTask;
        }

        public Task ClearAsync(WidgetKind kind, CancellationToken cancellationToken = default)
        {
            ClearedKinds.Add(kind);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingWidgetRefreshService : IWidgetRefreshService
    {
        public List<Guid> RefreshedUsers { get; } = [];

        public List<Guid> ClearedUsers { get; } = [];

        public Task RefreshAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            RefreshedUsers.Add(userId);
            return Task.CompletedTask;
        }

        public Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            ClearedUsers.Add(userId);
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

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class FakeItemRepository(IReadOnlyList<Item> seedItems) : IItemRepository
    {
        private readonly Dictionary<Guid, Item> items = seedItems.ToDictionary(item => item.Id);

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

    private sealed class FakeUserSettingsRepository(UserSettings? settings = null) : IUserSettingsRepository
    {
        public UserSettings? Current { get; private set; } = settings;

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

    private sealed class FakeAiChatMessageRepository(IReadOnlyList<AiChatMessage> seedMessages) : IAiChatMessageRepository
    {
        private readonly List<AiChatMessage> messages = [.. seedMessages];

        public Task<IReadOnlyList<AiChatMessage>> ListByDateRangeAsync(
            Guid userId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AiChatMessage>>(messages
                .Where(message => message.UserId == userId && message.OccurredOn >= startDate && message.OccurredOn <= endDate)
                .ToArray());
        }

        public Task UpsertAsync(AiChatMessage message, CancellationToken cancellationToken = default)
        {
            messages.RemoveAll(existing => existing.Id == message.Id);
            messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemorySyncChangeRepository : ISyncChangeRepository
    {
        public Task<IReadOnlyList<SyncChange>> ListPendingAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SyncChange>>([]);
        }

        public Task UpsertAsync(SyncChange change, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task MarkSyncedAsync(IEnumerable<Guid> changeIds, DateTimeOffset syncedAt, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(IEnumerable<Guid> changeIds, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
