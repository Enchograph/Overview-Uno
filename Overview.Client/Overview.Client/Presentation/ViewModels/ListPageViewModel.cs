using Overview.Client.Application.Auth;
using Overview.Client.Application.Lists;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Presentation.ViewModels;

public sealed class ListPageViewModel
{
    private readonly IAuthenticationService authenticationService;
    private readonly IListPageService listPageService;

    public ListPageViewModel(
        IAuthenticationService authenticationService,
        IListPageService listPageService)
    {
        this.authenticationService = authenticationService;
        this.listPageService = listPageService;
        Tabs = BuildTabs(ListPageTab.MyDay);
    }

    public IReadOnlyList<ListPageTabEntryViewModel> Tabs { get; private set; }

    public IReadOnlyList<ListPageItemEntryViewModel> ActiveItems { get; private set; } = Array.Empty<ListPageItemEntryViewModel>();

    public IReadOnlyList<ListPageItemEntryViewModel> CompletedItems { get; private set; } = Array.Empty<ListPageItemEntryViewModel>();

    public bool IsBusy { get; private set; }

    public bool IsAuthenticated => authenticationService.CurrentSession is not null;

    public ListPageTab CurrentTab { get; private set; } = ListPageTab.MyDay;

    public string PageTitle { get; private set; } = "List";

    public string PageSubtitle { get; private set; } =
        "Switch between tabs to filter tasks, schedules, notes, and important items.";

    public string StatusMessage { get; private set; } = "Loading list page.";

    public string ActiveSummary { get; private set; } = string.Empty;

    public string CompletedSummary { get; private set; } = string.Empty;

    public string EmptyStateTitle { get; private set; } = "No items yet";

    public string EmptyStateDescription { get; private set; } =
        "Create your first item from the add page, then return here to filter it by tab.";

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
                Tab = CurrentTab
            },
            cancellationToken).ConfigureAwait(false);
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
                Tabs = BuildTabs(CurrentTab);
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
            Tabs = BuildTabs(snapshot.Tab);
            ActiveItems = snapshot.ActiveItems.Select(ToEntry).ToArray();
            CompletedItems = snapshot.CompletedItems.Select(ToEntry).ToArray();
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
            ActiveItems = Array.Empty<ListPageItemEntryViewModel>();
            CompletedItems = Array.Empty<ListPageItemEntryViewModel>();
            ActiveSummary = string.Empty;
            CompletedSummary = string.Empty;
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static ListPageItemEntryViewModel ToEntry(ListPageItem item)
    {
        return new ListPageItemEntryViewModel
        {
            ItemId = item.ItemId,
            Title = item.Title,
            Subtitle = BuildItemSubtitle(item),
            TypeBadge = GetTypeBadge(item.Type),
            ImportanceBadge = item.IsImportant ? "Important" : string.Empty,
            CompletionBadge = item.IsCompleted ? "Completed" : "Open"
        };
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
