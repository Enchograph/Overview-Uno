using Overview.Client.Application.Auth;
using Overview.Client.Application.Items;
using Overview.Client.Application.Lists;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Presentation.ViewModels;

public sealed class ListPageViewModel
{
    private readonly IAuthenticationService authenticationService;
    private readonly IItemService itemService;
    private readonly IListPageService listPageService;
    private IReadOnlyList<ListPageItem> activeSnapshotItems = Array.Empty<ListPageItem>();
    private IReadOnlyList<ListPageItem> completedSnapshotItems = Array.Empty<ListPageItem>();

    public ListPageViewModel(
        IAuthenticationService authenticationService,
        IItemService itemService,
        IListPageService listPageService)
    {
        this.authenticationService = authenticationService;
        this.itemService = itemService;
        this.listPageService = listPageService;
        Tabs = BuildTabs(ListPageTab.MyDay);
        SortOptions = BuildSortOptions(ListSortBy.Importance);
        ThemeOptions = BuildThemeOptions("default");
    }

    public IReadOnlyList<ListPageTabEntryViewModel> Tabs { get; private set; }

    public IReadOnlyList<ListPageItemEntryViewModel> ActiveItems { get; private set; } = Array.Empty<ListPageItemEntryViewModel>();

    public IReadOnlyList<ListPageItemEntryViewModel> CompletedItems { get; private set; } = Array.Empty<ListPageItemEntryViewModel>();

    public IReadOnlyList<ListPageSortOptionViewModel> SortOptions { get; private set; }

    public IReadOnlyList<ListPageThemeOptionViewModel> ThemeOptions { get; private set; }

    public bool IsBusy { get; private set; }

    public bool IsAuthenticated => authenticationService.CurrentSession is not null;

    public ListPageTab CurrentTab { get; private set; } = ListPageTab.MyDay;

    public ListSortBy CurrentSortBy { get; private set; } = ListSortBy.Importance;

    public string PageTitle { get; private set; } = "List";

    public string PageSubtitle { get; private set; } =
        "Switch between tabs to filter tasks, schedules, notes, and important items.";

    public string StatusMessage { get; private set; } = "Loading list page.";

    public bool IsReorderMode { get; private set; }

    public string ReorderButtonLabel => IsReorderMode ? "Finish Reordering" : "Reorder Tasks";

    public string ActiveSummary { get; private set; } = string.Empty;

    public string CompletedSummary { get; private set; } = string.Empty;

    public string EmptyStateTitle { get; private set; } = "No items yet";

    public string EmptyStateDescription { get; private set; } =
        "Create your first item from the add page, then return here to filter it by tab.";

    public string CurrentTheme { get; private set; } = "default";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await LoadSnapshotAsync(query: null, cancellationToken).ConfigureAwait(false);
    }

    public async Task SelectTabAsync(ListPageTab tab, CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        if (tab == CurrentTab && (ActiveItems.Count > 0 || CompletedItems.Count > 0 || !IsAuthenticated))
        {
            return;
        }

        await LoadSnapshotAsync(
            new ListPageQuery
            {
                Tab = tab
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await LoadSnapshotAsync(
            new ListPageQuery
            {
                Tab = CurrentTab,
                SortBy = CurrentSortBy
            },
            cancellationToken).ConfigureAwait(false);
    }

    public void ToggleReorderMode()
    {
        IsReorderMode = !IsReorderMode;
        RebuildVisibleItems();
        StatusMessage = IsReorderMode
            ? "Reorder mode is on. Use the arrow buttons to move items within each section."
            : $"{PageTitle} reorder mode is off.";
    }

    public async Task SelectSortAsync(ListSortBy sortBy, CancellationToken cancellationToken = default)
    {
        if (IsBusy || sortBy == CurrentSortBy)
        {
            return;
        }

        var session = authenticationService.CurrentSession;
        if (session is null)
        {
            CurrentSortBy = sortBy;
            SortOptions = BuildSortOptions(sortBy);
            StatusMessage = "Sign in to save and apply list sorting.";
            return;
        }

        IsBusy = true;

        try
        {
            await listPageService.SetSortByAsync(session.UserId, sortBy, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await LoadSnapshotAsync(
            new ListPageQuery
            {
                Tab = CurrentTab,
                SortBy = sortBy
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task SelectThemeAsync(string themeKey, CancellationToken cancellationToken = default)
    {
        var normalizedTheme = NormalizeTheme(themeKey);
        if (IsBusy || string.Equals(CurrentTheme, normalizedTheme, StringComparison.Ordinal))
        {
            return;
        }

        var session = authenticationService.CurrentSession;
        if (session is null)
        {
            CurrentTheme = normalizedTheme;
            ThemeOptions = BuildThemeOptions(normalizedTheme);
            StatusMessage = "Sign in to save and apply list themes.";
            return;
        }

        IsBusy = true;

        try
        {
            await listPageService.SetThemeAsync(session.UserId, normalizedTheme, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync(cancellationToken).ConfigureAwait(false);
        StatusMessage = $"List theme switched to {GetThemeLabel(CurrentTheme)}.";
    }

    public async Task ToggleCompletionAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        var session = authenticationService.CurrentSession;
        if (session is null)
        {
            StatusMessage = "Sign in to update completion state.";
            return;
        }

        var item = FindItem(itemId);
        if (item is null)
        {
            StatusMessage = "Selected item is no longer available in this list.";
            return;
        }

        IsBusy = true;

        try
        {
            await itemService.SetCompletedAsync(session.UserId, itemId, !item.IsCompleted, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MoveItemUpAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        await ReorderItemAsync(itemId, moveUp: true, cancellationToken).ConfigureAwait(false);
    }

    public async Task MoveItemDownAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        await ReorderItemAsync(itemId, moveUp: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task ToggleImportanceAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        var session = authenticationService.CurrentSession;
        if (session is null)
        {
            StatusMessage = "Sign in to update important items.";
            return;
        }

        var item = FindItem(itemId);
        if (item is null)
        {
            StatusMessage = "Selected item is no longer available in this list.";
            return;
        }

        IsBusy = true;

        try
        {
            await itemService.SetImportantAsync(session.UserId, itemId, !item.IsImportant, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task LoadSnapshotAsync(
        ListPageQuery? query,
        CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var session = authenticationService.CurrentSession;
            if (session is null)
            {
                CurrentTab = query?.Tab ?? CurrentTab;
                CurrentSortBy = query?.SortBy ?? CurrentSortBy;
                Tabs = BuildTabs(CurrentTab);
                SortOptions = BuildSortOptions(CurrentSortBy);
                ThemeOptions = BuildThemeOptions(CurrentTheme);
                activeSnapshotItems = Array.Empty<ListPageItem>();
                completedSnapshotItems = Array.Empty<ListPageItem>();
                ActiveItems = Array.Empty<ListPageItemEntryViewModel>();
                CompletedItems = Array.Empty<ListPageItemEntryViewModel>();
                PageTitle = GetTabTitle(CurrentTab);
                PageSubtitle = GetTabSubtitle(CurrentTab);
                ActiveSummary = string.Empty;
                CompletedSummary = string.Empty;
                EmptyStateTitle = "Sign in to load your list";
                EmptyStateDescription = "List filtering is available after authentication restores your local account session.";
                StatusMessage = "List page requires an authenticated account.";
                return;
            }

            var snapshot = await listPageService.BuildSnapshotAsync(
                session.UserId,
                query,
                cancellationToken).ConfigureAwait(false);

            CurrentTab = snapshot.Tab;
            CurrentSortBy = snapshot.SortBy;
            CurrentTheme = NormalizeTheme(snapshot.Theme);
            Tabs = BuildTabs(snapshot.Tab);
            SortOptions = BuildSortOptions(snapshot.SortBy);
            ThemeOptions = BuildThemeOptions(CurrentTheme);
            activeSnapshotItems = snapshot.ActiveItems.ToArray();
            completedSnapshotItems = snapshot.CompletedItems.ToArray();
            RebuildVisibleItems();
            PageTitle = GetTabTitle(snapshot.Tab);
            PageSubtitle = GetTabSubtitle(snapshot.Tab);
            ActiveSummary = $"{ActiveItems.Count} active";
            CompletedSummary = $"{CompletedItems.Count} completed";
            EmptyStateTitle = $"No items in {PageTitle}";
            EmptyStateDescription = GetEmptyStateDescription(snapshot.Tab);
            StatusMessage = ActiveItems.Count == 0 && CompletedItems.Count == 0
                ? $"{PageTitle} is ready, but nothing matches this filter yet."
                : $"{PageTitle} loaded with {ActiveItems.Count + CompletedItems.Count} matching items.";
        }
        catch (Exception ex)
        {
            activeSnapshotItems = Array.Empty<ListPageItem>();
            completedSnapshotItems = Array.Empty<ListPageItem>();
            ActiveItems = Array.Empty<ListPageItemEntryViewModel>();
            CompletedItems = Array.Empty<ListPageItemEntryViewModel>();
            ThemeOptions = BuildThemeOptions(CurrentTheme);
            ActiveSummary = string.Empty;
            CompletedSummary = string.Empty;
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReorderItemAsync(
        Guid itemId,
        bool moveUp,
        CancellationToken cancellationToken)
    {
        if (IsBusy || !IsReorderMode)
        {
            return;
        }

        var session = authenticationService.CurrentSession;
        if (session is null)
        {
            StatusMessage = "Sign in to save manual list ordering.";
            return;
        }

        var activeIds = activeSnapshotItems.Select(item => item.ItemId).ToList();
        var completedIds = completedSnapshotItems.Select(item => item.ItemId).ToList();
        if (!TryMove(activeIds, itemId, moveUp) &&
            !TryMove(completedIds, itemId, moveUp))
        {
            return;
        }

        IsBusy = true;

        try
        {
            await listPageService.ReorderAsync(
                session.UserId,
                CurrentTab,
                activeIds.Concat(completedIds).ToArray(),
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync(cancellationToken).ConfigureAwait(false);
        StatusMessage = "Manual list order saved.";
    }

    private void RebuildVisibleItems()
    {
        ActiveItems = activeSnapshotItems
            .Select((item, index) => ToEntry(item, index, activeSnapshotItems.Count))
            .ToArray();
        CompletedItems = completedSnapshotItems
            .Select((item, index) => ToEntry(item, index, completedSnapshotItems.Count))
            .ToArray();
    }

    private ListPageItemEntryViewModel ToEntry(
        ListPageItem item,
        int index,
        int totalCount)
    {
        return new ListPageItemEntryViewModel
        {
            ItemId = item.ItemId,
            IsCompleted = item.IsCompleted,
            IsImportant = item.IsImportant,
            Title = item.Title,
            Subtitle = BuildItemSubtitle(item),
            TypeBadge = GetTypeBadge(item.Type),
            ImportanceBadge = item.IsImportant ? "Important" : string.Empty,
            CompletionBadge = item.IsCompleted ? "Completed" : "Open",
            CompletionGlyph = item.IsCompleted ? "●" : "○",
            ImportanceGlyph = item.IsImportant ? "★" : "☆",
            CanMoveUp = IsReorderMode && index > 0,
            CanMoveDown = IsReorderMode && index < totalCount - 1
        };
    }

    private static bool TryMove(IList<Guid> itemIds, Guid itemId, bool moveUp)
    {
        var index = itemIds.IndexOf(itemId);
        if (index < 0)
        {
            return false;
        }

        var targetIndex = moveUp ? index - 1 : index + 1;
        if (targetIndex < 0 || targetIndex >= itemIds.Count)
        {
            return false;
        }

        (itemIds[index], itemIds[targetIndex]) = (itemIds[targetIndex], itemIds[index]);
        return true;
    }

    private ListPageItemEntryViewModel? FindItem(Guid itemId)
    {
        return ActiveItems.Concat(CompletedItems).FirstOrDefault(item => item.ItemId == itemId);
    }

    private static string BuildItemSubtitle(ListPageItem item)
    {
        var segments = new List<string>
        {
            GetTypeLabel(item.Type)
        };

        if (item.RelevantDate is DateOnly relevantDate)
        {
            segments.Add($"Planned {relevantDate:yyyy-MM-dd}");
        }

        if (item.DeadlineDate is DateOnly deadlineDate)
        {
            segments.Add($"Due {deadlineDate:yyyy-MM-dd}");
        }

        return string.Join(" · ", segments);
    }

    private static IReadOnlyList<ListPageTabEntryViewModel> BuildTabs(ListPageTab selectedTab)
    {
        var tabs = new[]
        {
            ListPageTab.MyDay,
            ListPageTab.AllItems,
            ListPageTab.Tasks,
            ListPageTab.Schedules,
            ListPageTab.Notes,
            ListPageTab.Important
        };

        return tabs.Select(tab => new ListPageTabEntryViewModel
        {
            Key = tab.ToString(),
            Label = tab == selectedTab ? $"• {GetTabTitle(tab)}" : GetTabTitle(tab),
            Tab = tab,
            IsSelected = tab == selectedTab
        }).ToArray();
    }

    private static IReadOnlyList<ListPageSortOptionViewModel> BuildSortOptions(ListSortBy selectedSort)
    {
        var options = new[]
        {
            ListSortBy.Importance,
            ListSortBy.DueDate,
            ListSortBy.TodayExecutor,
            ListSortBy.Alphabetical,
            ListSortBy.CreatedAt
        };

        return options.Select(sortBy => new ListPageSortOptionViewModel
        {
            SortBy = sortBy,
            Label = GetSortLabel(sortBy),
            IsSelected = sortBy == selectedSort
        }).ToArray();
    }

    private static IReadOnlyList<ListPageThemeOptionViewModel> BuildThemeOptions(string selectedTheme)
    {
        return
        [
            CreateThemeOption("default", "Default", selectedTheme),
            CreateThemeOption("sunrise", "Sunrise", selectedTheme),
            CreateThemeOption("forest", "Forest", selectedTheme),
            CreateThemeOption("slate", "Slate", selectedTheme)
        ];
    }

    private static string GetTabTitle(ListPageTab tab)
    {
        return tab switch
        {
            ListPageTab.MyDay => "My Day",
            ListPageTab.AllItems => "All Items",
            ListPageTab.Tasks => "Tasks",
            ListPageTab.Schedules => "Schedules",
            ListPageTab.Notes => "Notes",
            ListPageTab.Important => "Important",
            _ => "List"
        };
    }

    private static string GetTabSubtitle(ListPageTab tab)
    {
        return tab switch
        {
            ListPageTab.MyDay => "Items relevant to today in your configured timezone.",
            ListPageTab.AllItems => "Every non-deleted item across schedules, tasks, and notes.",
            ListPageTab.Tasks => "Only task items with their planned range and deadline metadata.",
            ListPageTab.Schedules => "Only scheduled calendar blocks.",
            ListPageTab.Notes => "Only memo-style notes.",
            ListPageTab.Important => "Items currently marked as important.",
            _ => "Filter your items by list tab."
        };
    }

    private static string GetEmptyStateDescription(ListPageTab tab)
    {
        return tab switch
        {
            ListPageTab.MyDay => "Nothing is scheduled for today. Add a dated task, schedule, or note to populate this tab.",
            ListPageTab.AllItems => "Create an item from the add page to start building your full list.",
            ListPageTab.Tasks => "Create a task with a planned range and deadline to show it here.",
            ListPageTab.Schedules => "Create a schedule item to show timed events here.",
            ListPageTab.Notes => "Create a note to keep lightweight reminders here.",
            ListPageTab.Important => "Mark an item as important to surface it in this tab.",
            _ => "No matching items."
        };
    }

    private static string GetSortLabel(ListSortBy sortBy)
    {
        return sortBy switch
        {
            ListSortBy.Importance => "Importance",
            ListSortBy.DueDate => "Due Date",
            ListSortBy.TodayExecutor => "Today Executor",
            ListSortBy.Alphabetical => "Alphabetical",
            ListSortBy.CreatedAt => "Created Date",
            _ => sortBy.ToString()
        };
    }

    private static ListPageThemeOptionViewModel CreateThemeOption(
        string themeKey,
        string label,
        string selectedTheme)
    {
        return new ListPageThemeOptionViewModel
        {
            ThemeKey = themeKey,
            Label = label,
            IsSelected = string.Equals(themeKey, selectedTheme, StringComparison.Ordinal)
        };
    }

    private static string NormalizeTheme(string? themeKey)
    {
        if (string.IsNullOrWhiteSpace(themeKey))
        {
            return "default";
        }

        return themeKey.Trim().ToLowerInvariant() switch
        {
            "sunrise" => "sunrise",
            "forest" => "forest",
            "slate" => "slate",
            _ => "default"
        };
    }

    private static string GetThemeLabel(string themeKey)
    {
        return NormalizeTheme(themeKey) switch
        {
            "sunrise" => "Sunrise",
            "forest" => "Forest",
            "slate" => "Slate",
            _ => "Default"
        };
    }

    private static string GetTypeLabel(ItemType type)
    {
        return type switch
        {
            ItemType.Schedule => "Schedule",
            ItemType.Task => "Task",
            ItemType.Note => "Note",
            _ => type.ToString()
        };
    }

    private static string GetTypeBadge(ItemType type)
    {
        return type switch
        {
            ItemType.Schedule => "S",
            ItemType.Task => "T",
            ItemType.Note => "N",
            _ => "?"
        };
    }
}
