using Overview.Client.Application.Auth;
using Overview.Client.Application.Items;
using Overview.Client.Application.Lists;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Tests;

public sealed class ListPageViewModelTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TaskItemId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid ScheduleItemId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task SelectSortAsync_UpdatesCurrentSortAndReloadsSnapshot()
    {
        var itemService = new FakeItemService();
        var listPageService = new FakeListPageService();
        var viewModel = CreateViewModel(itemService, listPageService);

        await viewModel.InitializeAsync();
        await viewModel.SelectSortAsync(ListSortBy.CreatedAt);

        Assert.Equal(ListSortBy.CreatedAt, viewModel.CurrentSortBy);
        Assert.Equal(ListSortBy.CreatedAt, listPageService.LastSortBy);
        Assert.Equal("Created Date", viewModel.SortOptions.Single(option => option.IsSelected).Label);
    }

    [Fact]
    public async Task ToggleCompletionAsync_MovesItemIntoCompletedGroup()
    {
        var itemService = new FakeItemService();
        var listPageService = new FakeListPageService();
        var viewModel = CreateViewModel(itemService, listPageService);

        await viewModel.InitializeAsync();
        await viewModel.ToggleCompletionAsync(TaskItemId);

        Assert.Contains(viewModel.CompletedItems, item => item.ItemId == TaskItemId && item.IsCompleted);
        Assert.DoesNotContain(viewModel.ActiveItems, item => item.ItemId == TaskItemId);
    }

    [Fact]
    public async Task ToggleImportanceAsync_RemovesItemFromImportantTabWhenUnset()
    {
        var itemService = new FakeItemService();
        var listPageService = new FakeListPageService();
        var viewModel = CreateViewModel(itemService, listPageService);

        await viewModel.InitializeAsync();
        await viewModel.SelectTabAsync(ListPageTab.Important);
        await viewModel.ToggleImportanceAsync(ScheduleItemId);

        Assert.Empty(viewModel.ActiveItems);
        Assert.Equal(ListPageTab.Important, viewModel.CurrentTab);
    }

    [Fact]
    public async Task MoveItemDownAsync_SavesManualOrderAndRefreshesVisibleSequence()
    {
        var itemService = new FakeItemService();
        var listPageService = new FakeListPageService();
        var viewModel = CreateViewModel(itemService, listPageService);

        await viewModel.InitializeAsync();
        await viewModel.SelectSortAsync(ListSortBy.Alphabetical);
        viewModel.ToggleReorderMode();
        await viewModel.MoveItemDownAsync(TaskItemId);

        Assert.Equal([ScheduleItemId, TaskItemId], viewModel.ActiveItems.Select(item => item.ItemId));
        Assert.Equal([ScheduleItemId, TaskItemId], listPageService.LastReorderedIds);
        Assert.False(viewModel.ActiveItems[0].CanMoveUp);
        Assert.True(viewModel.ActiveItems[0].CanMoveDown);
    }

    [Fact]
    public async Task SelectThemeAsync_UpdatesCurrentThemeAndReloadsOptions()
    {
        var itemService = new FakeItemService();
        var listPageService = new FakeListPageService();
        var viewModel = CreateViewModel(itemService, listPageService);

        await viewModel.InitializeAsync();
        await viewModel.SelectThemeAsync("forest");

        Assert.Equal("forest", viewModel.CurrentTheme);
        Assert.Equal("forest", listPageService.LastTheme);
        Assert.Equal("Forest", viewModel.ThemeOptions.Single(option => option.IsSelected).Label);
    }

    [Fact]
    public async Task DeleteItemAsync_RemovesItemFromVisibleSnapshot()
    {
        var itemService = new FakeItemService();
        var listPageService = new FakeListPageService();
        var viewModel = CreateViewModel(itemService, listPageService);

        await viewModel.InitializeAsync();
        await viewModel.DeleteItemAsync(TaskItemId);

        Assert.DoesNotContain(viewModel.ActiveItems, item => item.ItemId == TaskItemId);
        Assert.Contains("Deleted Alpha Task.", viewModel.StatusMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAddNavigationRequest_UsesCurrentTabDefaults()
    {
        var itemService = new FakeItemService();
        var listPageService = new FakeListPageService();
        var viewModel = CreateViewModel(itemService, listPageService);

        await viewModel.InitializeAsync();
        var myDayRequest = viewModel.CreateAddNavigationRequest();

        await viewModel.SelectTabAsync(ListPageTab.Tasks);
        var taskRequest = viewModel.CreateAddNavigationRequest();

        await viewModel.SelectTabAsync(ListPageTab.Important);
        var importantRequest = viewModel.CreateAddNavigationRequest();

        Assert.Equal(new DateOnly(2026, 3, 13), myDayRequest.SuggestedStartDate);
        Assert.Equal(ItemType.Task, taskRequest.SuggestedType);
        Assert.True(importantRequest.SuggestedIsImportant);
    }

    private static ListPageViewModel CreateViewModel(FakeItemService itemService, FakeListPageService listPageService)
    {
        itemService.ListPageService = listPageService;

        return new ListPageViewModel(
            new FakeAuthenticationService(),
            itemService,
            listPageService);
    }

    private sealed class FakeListPageService : IListPageService
    {
        private readonly List<FakeListItem> items =
        [
            new(TaskItemId, ItemType.Task, "Alpha Task", false, false, new DateOnly(2026, 3, 13), new DateOnly(2026, 3, 13), new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero)),
            new(ScheduleItemId, ItemType.Schedule, "Beta Schedule", true, false, new DateOnly(2026, 3, 13), new DateOnly(2026, 3, 13), new DateTimeOffset(2026, 3, 9, 0, 0, 0, TimeSpan.Zero))
        ];

        public ListSortBy LastSortBy { get; private set; } = ListSortBy.Importance;

        public IReadOnlyList<Guid> LastReorderedIds { get; private set; } = Array.Empty<Guid>();

        public string LastTheme { get; private set; } = "default";

        public Task<ListPageSnapshot> BuildSnapshotAsync(Guid userId, ListPageQuery? query = null, CancellationToken cancellationToken = default)
        {
            var tab = query?.Tab ?? ListPageTab.MyDay;
            var sortBy = query?.SortBy ?? LastSortBy;
            LastSortBy = sortBy;

            IEnumerable<FakeListItem> filtered = items.Where(item => tab switch
            {
                ListPageTab.Important => item.IsImportant,
                ListPageTab.Tasks => item.Type == ItemType.Task,
                ListPageTab.Schedules => item.Type == ItemType.Schedule,
                _ => true
            });

            filtered = sortBy switch
            {
                ListSortBy.CreatedAt => filtered.OrderByDescending(item => item.LastModifiedAt),
                ListSortBy.Alphabetical => filtered.OrderBy(item => item.Title, StringComparer.Ordinal),
                _ => filtered.OrderBy(item => item.Type)
            };

            if (LastReorderedIds.Count > 0)
            {
                var orderedLookup = LastReorderedIds
                    .Select((itemId, index) => new { itemId, index })
                    .ToDictionary(entry => entry.itemId, entry => entry.index);
                filtered = filtered
                    .OrderBy(item => orderedLookup.TryGetValue(item.ItemId, out var index) ? index : int.MaxValue)
                    .ThenBy(item => item.Title, StringComparer.Ordinal);
            }

            return Task.FromResult(new ListPageSnapshot
            {
                Tab = tab,
                SortBy = sortBy,
                ReferenceDate = new DateOnly(2026, 3, 13),
                Theme = LastTheme,
                ActiveItems = filtered.Where(item => !item.IsCompleted).Select(ToItem).ToArray(),
                CompletedItems = filtered.Where(item => item.IsCompleted).Select(ToItem).ToArray()
            });
        }

        public Task<UserSettings> SetSortByAsync(Guid userId, ListSortBy sortBy, CancellationToken cancellationToken = default)
        {
            LastSortBy = sortBy;
            return Task.FromResult(new UserSettings
            {
                UserId = userId,
                ListPageSortBy = sortBy
            });
        }

        public Task<UserSettings> ReorderAsync(Guid userId, ListPageTab tab, IReadOnlyList<Guid> orderedItemIds, CancellationToken cancellationToken = default)
        {
            LastReorderedIds = orderedItemIds.ToArray();

            var orderedLookup = orderedItemIds
                .Select((itemId, index) => new { itemId, index })
                .ToDictionary(entry => entry.itemId, entry => entry.index);

            items.Sort((left, right) =>
            {
                var leftRank = orderedLookup.TryGetValue(left.ItemId, out var leftIndex) ? leftIndex : int.MaxValue;
                var rightRank = orderedLookup.TryGetValue(right.ItemId, out var rightIndex) ? rightIndex : int.MaxValue;
                return leftRank.CompareTo(rightRank);
            });

            return Task.FromResult(new UserSettings
            {
                UserId = userId
            });
        }

        public Task<UserSettings> SetThemeAsync(Guid userId, string theme, CancellationToken cancellationToken = default)
        {
            LastTheme = theme;
            return Task.FromResult(new UserSettings
            {
                UserId = userId,
                ListPageTheme = theme
            });
        }

        public void SetCompleted(Guid itemId, bool isCompleted)
        {
            var index = items.FindIndex(item => item.ItemId == itemId);
            items[index] = items[index] with { IsCompleted = isCompleted };
        }

        public void SetImportant(Guid itemId, bool isImportant)
        {
            var index = items.FindIndex(item => item.ItemId == itemId);
            items[index] = items[index] with { IsImportant = isImportant };
        }

        public void Delete(Guid itemId)
        {
            items.RemoveAll(item => item.ItemId == itemId);
        }

        private static ListPageItem ToItem(FakeListItem item)
        {
            return new ListPageItem
            {
                ItemId = item.ItemId,
                Type = item.Type,
                Title = item.Title,
                IsImportant = item.IsImportant,
                IsCompleted = item.IsCompleted,
                RelevantDate = item.RelevantDate,
                DeadlineDate = item.DeadlineDate,
                LastModifiedAt = item.LastModifiedAt
            };
        }
    }

    private sealed class FakeItemService : IItemService
    {
        public FakeListPageService? ListPageService { get; set; }

        public Task<Item?> GetAsync(Guid userId, Guid itemId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Item>> ListAsync(Guid userId, ItemQueryOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
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
            ListPageService?.SetCompleted(itemId, isCompleted);
            return Task.FromResult(new Item
            {
                Id = itemId,
                UserId = userId,
                IsCompleted = isCompleted,
                SourceDeviceId = "test-device",
                Title = "Updated",
                Type = ItemType.Task,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastModifiedAt = DateTimeOffset.UtcNow
            });
        }

        public Task<Item> SetImportantAsync(Guid userId, Guid itemId, bool isImportant, CancellationToken cancellationToken = default)
        {
            ListPageService?.SetImportant(itemId, isImportant);
            return Task.FromResult(new Item
            {
                Id = itemId,
                UserId = userId,
                IsImportant = isImportant,
                SourceDeviceId = "test-device",
                Title = "Updated",
                Type = ItemType.Task,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastModifiedAt = DateTimeOffset.UtcNow
            });
        }

        public Task DeleteAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
        {
            ListPageService?.Delete(itemId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public AuthSession? CurrentSession { get; } = new()
        {
            Mode = AuthenticationMode.OfflineLocal,
            UserId = UserId,
            Email = "test@example.com",
            RestoredAt = DateTimeOffset.UtcNow
        };

        public bool IsAuthenticated => true;

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

        public Task<AuthSession> LoginOfflineAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession?> RestoreSessionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession> RefreshSessionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed record FakeListItem(
        Guid ItemId,
        ItemType Type,
        string Title,
        bool IsImportant,
        bool IsCompleted,
        DateOnly? RelevantDate,
        DateOnly? DeadlineDate,
        DateTimeOffset LastModifiedAt);
}
