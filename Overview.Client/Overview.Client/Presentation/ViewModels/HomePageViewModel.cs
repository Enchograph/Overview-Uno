using Overview.Client.Application.Auth;
using Overview.Client.Application.Home;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Presentation.ViewModels;

public sealed class HomePageViewModel
{
    private const double WideLayoutThreshold = 960d;

    private readonly IAuthenticationService authenticationService;
    private readonly IHomeLayoutService homeLayoutService;

    public HomePageViewModel(
        IAuthenticationService authenticationService,
        IHomeLayoutService homeLayoutService)
    {
        this.authenticationService = authenticationService;
        this.homeLayoutService = homeLayoutService;
    }

    public string Title => "Home";

    public string Description =>
        "Overview timeline grid for week and month planning with scheduled item overlays.";

    public bool IsBusy { get; private set; }

    public bool IsAuthenticated => authenticationService.CurrentSession is not null;

    public bool SupportsMonthView { get; private set; }

    public bool IsPickerOpen { get; private set; }

    public HomeViewMode CurrentViewMode { get; private set; } = HomeViewMode.Week;

    public DateOnly CurrentReferenceDate { get; private set; } = DateOnly.FromDateTime(DateTime.Now.Date);

    public string PeriodTitle { get; private set; } = "Home";

    public string ViewModeSummary { get; private set; } = "Week view ready.";

    public string StatusMessage { get; private set; } = "Loading home timeline.";

    public string VisibleRangeSummary { get; private set; } = string.Empty;

    public string GridSummary { get; private set; } = string.Empty;

    public HomeLayoutSnapshot? Snapshot { get; private set; }

    public TimeSelectionMode CurrentSelectionMode =>
        CurrentViewMode == HomeViewMode.Month ? TimeSelectionMode.Month : TimeSelectionMode.Week;

    public async Task InitializeAsync(double viewportWidth, CancellationToken cancellationToken = default)
    {
        SupportsMonthView = viewportWidth >= WideLayoutThreshold;
        await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateViewportAsync(double viewportWidth, CancellationToken cancellationToken = default)
    {
        var supportsMonthView = viewportWidth >= WideLayoutThreshold;
        if (supportsMonthView == SupportsMonthView)
        {
            return;
        }

        SupportsMonthView = supportsMonthView;
        if (!SupportsMonthView && CurrentViewMode == HomeViewMode.Month)
        {
            CurrentViewMode = HomeViewMode.Week;
        }

        await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task NavigatePreviousAsync(CancellationToken cancellationToken = default)
    {
        if (Snapshot is null)
        {
            return;
        }

        CurrentReferenceDate = Snapshot.Period.StartDate.AddDays(-1);
        await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task NavigateNextAsync(CancellationToken cancellationToken = default)
    {
        if (Snapshot is null)
        {
            return;
        }

        CurrentReferenceDate = Snapshot.Period.EndDate.AddDays(1);
        await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SetViewModeAsync(HomeViewMode viewMode, CancellationToken cancellationToken = default)
    {
        if (viewMode == HomeViewMode.Month && !SupportsMonthView)
        {
            StatusMessage = "Month view unlocks on wider layouts. Week view remains active on narrow screens.";
            return;
        }

        if (CurrentViewMode == viewMode && Snapshot is not null)
        {
            return;
        }

        CurrentViewMode = viewMode;
        await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public void TogglePicker()
    {
        IsPickerOpen = !IsPickerOpen;
    }

    public void ClosePicker()
    {
        IsPickerOpen = false;
    }

    public async Task ApplyConfirmedPeriodAsync(CalendarPeriod period, CancellationToken cancellationToken = default)
    {
        CurrentReferenceDate = period.ReferenceDate;
        CurrentViewMode = period.Mode == TimeSelectionMode.Month ? HomeViewMode.Month : HomeViewMode.Week;
        IsPickerOpen = false;
        await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task LoadSnapshotAsync(CancellationToken cancellationToken)
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
                Snapshot = null;
                PeriodTitle = "Home";
                VisibleRangeSummary = string.Empty;
                GridSummary = string.Empty;
                ViewModeSummary = "Sign in to load your planning timeline.";
                StatusMessage = "Home timeline requires an authenticated account.";
                return;
            }

            if (!SupportsMonthView && CurrentViewMode == HomeViewMode.Month)
            {
                CurrentViewMode = HomeViewMode.Week;
            }

            var snapshot = await homeLayoutService.BuildSnapshotAsync(
                session.UserId,
                CurrentReferenceDate,
                CurrentViewMode,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            Snapshot = snapshot;
            CurrentReferenceDate = snapshot.Period.ReferenceDate;
            PeriodTitle = snapshot.Title;
            VisibleRangeSummary =
                $"{snapshot.Period.StartDate:yyyy-MM-dd} to {snapshot.Period.EndDate:yyyy-MM-dd}";
            ViewModeSummary = SupportsMonthView
                ? $"{snapshot.ViewMode} view active. Resize narrower to fall back to week mode."
                : "Narrow layout active. Week view is enforced until the page is wide enough for month mode.";
            GridSummary =
                $"{snapshot.Columns.Count} day columns · {snapshot.TimeBlocks.Count} time blocks · {snapshot.Items.Count} scheduled items rendered in the visible range";
            StatusMessage = snapshot.Items.Count == 0
                ? "Timeline grid ready. No scheduled items in the visible range yet."
                : "Timeline grid ready. Scheduled items now render with proportional height and overlap opacity.";
        }
        catch (Exception ex)
        {
            Snapshot = null;
            PeriodTitle = "Home";
            VisibleRangeSummary = string.Empty;
            GridSummary = string.Empty;
            ViewModeSummary = SupportsMonthView ? "Week or month view available." : "Week view ready.";
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
