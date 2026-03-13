using Overview.Client.Application.Items;
using Overview.Client.Application.Lists;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Tests;

public sealed class ListPageServiceTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly DateOnly ReferenceDate = new(2026, 3, 13);
    private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.Utc;

    [Theory]
    [InlineData(ListPageTab.MyDay, new[] { "Daily Task", "Focus Note", "Morning Schedule" })]
    [InlineData(ListPageTab.AllItems, new[] { "Daily Task", "Focus Note", "Future Task", "Morning Schedule", "Weekly Review" })]
    [InlineData(ListPageTab.Tasks, new[] { "Daily Task", "Future Task" })]
    [InlineData(ListPageTab.Schedules, new[] { "Morning Schedule" })]
    [InlineData(ListPageTab.Notes, new[] { "Focus Note", "Weekly Review" })]
    [InlineData(ListPageTab.Important, new[] { "Focus Note", "Morning Schedule" })]
    public async Task BuildSnapshotAsync_FiltersItemsByTab(ListPageTab tab, string[] expectedTitles)
    {
        var service = CreateService();

        var snapshot = await service.BuildSnapshotAsync(
            UserId,
            new ListPageQuery
            {
                Tab = tab,
                ReferenceDate = ReferenceDate
            });

        var titles = snapshot.ActiveItems
            .Concat(snapshot.CompletedItems)
            .Select(item => item.Title)
            .OrderBy(title => title, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedTitles.OrderBy(title => title, StringComparer.Ordinal), titles);
    }

    [Fact]
    public async Task BuildSnapshotAsync_SortsByImportanceBeforeNonImportantItems()
    {
        var service = CreateService();

        var snapshot = await service.BuildSnapshotAsync(
            UserId,
            new ListPageQuery
            {
                Tab = ListPageTab.AllItems,
                SortBy = ListSortBy.Importance,
                ReferenceDate = ReferenceDate
            });

        var titles = snapshot.ActiveItems.Select(item => item.Title).ToArray();

        Assert.Equal("Focus Note", titles[0]);
        Assert.Equal("Morning Schedule", titles[1]);
    }

    [Fact]
    public async Task BuildSnapshotAsync_SortsByDueDateAscending()
    {
        var service = CreateService();

        var snapshot = await service.BuildSnapshotAsync(
            UserId,
            new ListPageQuery
            {
                Tab = ListPageTab.AllItems,
                SortBy = ListSortBy.DueDate,
                ReferenceDate = ReferenceDate
            });

        var titles = snapshot.ActiveItems.Select(item => item.Title).ToArray();

        Assert.Equal(["Daily Task", "Focus Note", "Morning Schedule", "Future Task", "Weekly Review"], titles);
    }

    [Fact]
    public async Task ReorderAsync_PersistsManualOrderAndBuildSnapshotReflectsIt()
    {
        var settingsService = new FakeUserSettingsService(new UserSettings
        {
            UserId = UserId,
            TimeZoneId = TimeZone.Id,
            ListPageDefaultTab = ListPageTab.MyDay,
            ListPageSortBy = ListSortBy.Alphabetical,
            ListManualOrder = new ListManualOrderPreferences()
        });
        var service = CreateService(settingsService);

        var initialSnapshot = await service.BuildSnapshotAsync(
            UserId,
            new ListPageQuery
            {
                Tab = ListPageTab.AllItems,
                SortBy = ListSortBy.Alphabetical,
                ReferenceDate = ReferenceDate
            });

        var reorderedIds = initialSnapshot.ActiveItems
            .Select(item => item.ItemId)
            .Reverse()
            .ToArray();

        await service.ReorderAsync(UserId, ListPageTab.AllItems, reorderedIds);

        var reorderedSnapshot = await service.BuildSnapshotAsync(
            UserId,
            new ListPageQuery
            {
                Tab = ListPageTab.AllItems,
                SortBy = ListSortBy.Alphabetical,
                ReferenceDate = ReferenceDate
            });

        Assert.Equal(
            initialSnapshot.ActiveItems.Select(item => item.Title).Reverse(),
            reorderedSnapshot.ActiveItems.Select(item => item.Title));
    }

    [Fact]
    public async Task SetThemeAsync_PersistsThemeAndBuildSnapshotReflectsIt()
    {
        var settingsService = new FakeUserSettingsService(new UserSettings
        {
            UserId = UserId,
            TimeZoneId = TimeZone.Id,
            ListPageDefaultTab = ListPageTab.MyDay,
            ListPageSortBy = ListSortBy.Alphabetical,
            ListPageTheme = "default",
            ListManualOrder = new ListManualOrderPreferences()
        });
        var service = CreateService(settingsService);

        await service.SetThemeAsync(UserId, "forest");

        var snapshot = await service.BuildSnapshotAsync(
            UserId,
            new ListPageQuery
            {
                Tab = ListPageTab.AllItems,
                ReferenceDate = ReferenceDate
            });

        Assert.Equal("forest", snapshot.Theme);
    }

    private static ListPageService CreateService(FakeUserSettingsService? settingsService = null)
    {
        return new ListPageService(
            new FakeItemService(
            [
                CreateTask("Daily Task", plannedStartAt: new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero), plannedEndAt: new DateTimeOffset(2026, 3, 13, 9, 0, 0, TimeSpan.Zero), deadlineAt: new DateTimeOffset(2026, 3, 13, 18, 0, 0, TimeSpan.Zero)),
                CreateTask("Future Task", plannedStartAt: new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero), plannedEndAt: new DateTimeOffset(2026, 3, 15, 11, 0, 0, TimeSpan.Zero), deadlineAt: new DateTimeOffset(2026, 3, 16, 18, 0, 0, TimeSpan.Zero)),
                CreateSchedule("Morning Schedule", new DateTimeOffset(2026, 3, 13, 9, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 3, 13, 10, 0, 0, TimeSpan.Zero), isImportant: true),
                CreateNote("Focus Note", new DateOnly(2026, 3, 13), isImportant: true),
                CreateNote("Weekly Review", new DateOnly(2026, 3, 20))
            ]),
            settingsService ?? new FakeUserSettingsService(new UserSettings
            {
                UserId = UserId,
                TimeZoneId = TimeZone.Id,
                ListPageDefaultTab = ListPageTab.MyDay,
                ListPageSortBy = ListSortBy.Alphabetical,
                ListManualOrder = new ListManualOrderPreferences()
            }));
    }

    private static Item CreateTask(
        string title,
        DateTimeOffset plannedStartAt,
        DateTimeOffset plannedEndAt,
        DateTimeOffset deadlineAt,
        bool isImportant = false)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Type = ItemType.Task,
            Title = title,
            IsImportant = isImportant,
            TimeZoneId = TimeZone.Id,
            CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            LastModifiedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            SourceDeviceId = "test-device",
            PlannedStartAt = plannedStartAt,
            PlannedEndAt = plannedEndAt,
            DeadlineAt = deadlineAt
        };
    }

    private static Item CreateSchedule(
        string title,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        bool isImportant = false)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Type = ItemType.Schedule,
            Title = title,
            IsImportant = isImportant,
            TimeZoneId = TimeZone.Id,
            CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            LastModifiedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            SourceDeviceId = "test-device",
            StartAt = startAt,
            EndAt = endAt
        };
    }

    private static Item CreateNote(
        string title,
        DateOnly targetDate,
        bool isImportant = false)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Type = ItemType.Note,
            Title = title,
            IsImportant = isImportant,
            TimeZoneId = TimeZone.Id,
            CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            LastModifiedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            SourceDeviceId = "test-device",
            ExpectedDurationMinutes = 30,
            TargetDate = targetDate
        };
    }

    private sealed class FakeItemService(IReadOnlyList<Item> items) : IItemService
    {
        public Task<Item?> GetAsync(Guid userId, Guid itemId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(items.FirstOrDefault(item => item.UserId == userId && item.Id == itemId));
        }

        public Task<IReadOnlyList<Item>> ListAsync(Guid userId, ItemQueryOptions? options = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Item>>(items.Where(item => item.UserId == userId).ToArray());
        }

        public Task<Item> CreateAsync(Guid userId, ItemUpsertRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> UpdateAsync(Guid userId, Guid itemId, ItemUpsertRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> SetCompletedAsync(Guid userId, Guid itemId, bool isCompleted, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> SetImportantAsync(Guid userId, Guid itemId, bool isImportant, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeUserSettingsService(UserSettings initialSettings) : IUserSettingsService
    {
        private UserSettings settings = initialSettings;

        public Task<UserSettings> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(settings);
        }

        public Task<UserSettings> SaveAsync(Guid userId, UserSettingsUpdateRequest request, CancellationToken cancellationToken = default)
        {
            settings = new UserSettings
            {
                UserId = userId,
                Language = request.Language,
                ThemeMode = request.ThemeMode,
                ThemePreset = request.ThemePreset,
                WeekStartDay = request.WeekStartDay,
                HomeViewMode = request.HomeViewMode,
                DayPlanStartTime = request.DayPlanStartTime,
                TimeBlockDurationMinutes = request.TimeBlockDurationMinutes,
                TimeBlockGapMinutes = request.TimeBlockGapMinutes,
                TimeBlockCount = request.TimeBlockCount,
                ListPageDefaultTab = request.ListPageDefaultTab,
                ListPageSortBy = request.ListPageSortBy,
                ListPageTheme = request.ListPageTheme,
                ListManualOrder = request.ListManualOrder,
                AiBaseUrl = request.AiBaseUrl,
                AiApiKey = request.AiApiKey,
                AiModel = request.AiModel,
                SyncServerBaseUrl = request.SyncServerBaseUrl,
                NotificationEnabled = request.NotificationEnabled,
                WidgetPreferences = request.WidgetPreferences,
                TimeZoneId = request.TimeZoneId ?? settings.TimeZoneId
            };

            return Task.FromResult(settings);
        }
    }
}
