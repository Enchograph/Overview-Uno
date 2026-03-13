using System.Globalization;
using Overview.Client.Application.Auth;
using Overview.Client.Application.Home;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Presentation.ViewModels;

public sealed class TimeSelectionViewModel
{
    private readonly IAuthenticationService authenticationService;
    private readonly ITimeSelectionService timeSelectionService;
    private DateOnly anchorDate;

    public TimeSelectionViewModel(
        IAuthenticationService authenticationService,
        ITimeSelectionService timeSelectionService)
    {
        this.authenticationService = authenticationService;
        this.timeSelectionService = timeSelectionService;
        VisibleMonthLabel = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy/MM", CultureInfo.InvariantCulture);
        WeekdayHeaders = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        Weeks = Array.Empty<TimeSelectionWeekRowViewModel>();
        StatusMessage = "Loading time selection.";
        SelectedPeriodLabel = "No period selected.";
    }

    public TimeSelectionMode SelectionMode { get; private set; } = TimeSelectionMode.Week;

    public string VisibleMonthLabel { get; private set; }

    public string SelectedPeriodLabel { get; private set; }

    public string StatusMessage { get; private set; }

    public IReadOnlyList<string> WeekdayHeaders { get; private set; }

    public IReadOnlyList<TimeSelectionWeekRowViewModel> Weeks { get; private set; }

    public Visibility MonthSelectionIndicatorVisibility { get; private set; } = Visibility.Collapsed;

    public bool IsBusy { get; private set; }

    public CalendarPeriod? SelectedPeriod { get; private set; }

    public bool IsAuthenticated => authenticationService.CurrentSession is not null;

    public async Task InitializeAsync(
        TimeSelectionMode selectionMode,
        DateOnly? initialDate = null,
        CancellationToken cancellationToken = default)
    {
        anchorDate = initialDate ?? DateOnly.FromDateTime(DateTime.Today);
        SelectionMode = selectionMode;
        await RefreshSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ChangeSelectionModeAsync(
        TimeSelectionMode selectionMode,
        CancellationToken cancellationToken = default)
    {
        SelectionMode = selectionMode;
        await RefreshSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SelectDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        anchorDate = date;
        await RefreshSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SelectWeekAsync(CalendarPeriod weekPeriod, CancellationToken cancellationToken = default)
    {
        anchorDate = weekPeriod.ReferenceDate;
        await RefreshSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SelectMonthAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedPeriod is { Mode: TimeSelectionMode.Month } currentMonth)
        {
            anchorDate = currentMonth.ReferenceDate;
        }

        await RefreshSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MoveToPreviousMonthAsync(CancellationToken cancellationToken = default)
    {
        anchorDate = new DateOnly(anchorDate.Year, anchorDate.Month, 1).AddMonths(-1);
        await RefreshSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MoveToNextMonthAsync(CancellationToken cancellationToken = default)
    {
        anchorDate = new DateOnly(anchorDate.Year, anchorDate.Month, 1).AddMonths(1);
        await RefreshSnapshotAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task RefreshSnapshotAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            if (authenticationService.CurrentSession is not AuthSession session)
            {
                Weeks = Array.Empty<TimeSelectionWeekRowViewModel>();
                SelectedPeriod = null;
                SelectedPeriodLabel = "Sign in to use the time picker.";
                VisibleMonthLabel = anchorDate.ToString("yyyy/MM", CultureInfo.InvariantCulture);
                MonthSelectionIndicatorVisibility = Visibility.Collapsed;
                StatusMessage = "Authentication required.";
                return;
            }

            var visibleMonth = new DateOnly(anchorDate.Year, anchorDate.Month, 1);
            var snapshot = await timeSelectionService.BuildMonthSnapshotAsync(
                    session.UserId,
                    visibleMonth,
                    SelectionMode,
                    anchorDate,
                    cancellationToken)
                .ConfigureAwait(false);

            SelectedPeriod = snapshot.SelectedPeriod;
            VisibleMonthLabel = snapshot.HeaderLabel;
            SelectedPeriodLabel = snapshot.SelectedPeriod is null
                ? "No period selected."
                : $"{snapshot.SelectedPeriod.Mode}: {snapshot.SelectedPeriod.StartDate:yyyy-MM-dd} -> {snapshot.SelectedPeriod.EndDate:yyyy-MM-dd}";
            WeekdayHeaders = BuildWeekdayHeaders(snapshot);
            MonthSelectionIndicatorVisibility =
                SelectionMode == TimeSelectionMode.Month ? Visibility.Visible : Visibility.Collapsed;
            Weeks = snapshot.Weeks
                .Select((week, index) => new TimeSelectionWeekRowViewModel
                {
                    WeekLabel = $"Week {index + 1}",
                    WeekPeriod = week.WeekPeriod,
                    SelectionIndicatorVisibility =
                        SelectionMode == TimeSelectionMode.Week && week.IsSelected
                            ? Visibility.Visible
                            : Visibility.Collapsed,
                    Dates = week.Dates
                        .Select(date => new TimeSelectionDateCellViewModel
                        {
                            Date = date.Date,
                            DayNumberText = date.DayNumber.ToString(CultureInfo.InvariantCulture),
                            CaptionText = date.IsToday ? "Today" : string.Empty,
                            IsEnabled = date.IsInVisibleMonth,
                            IsToday = date.IsToday,
                            CellOpacity = date.IsInVisibleMonth ? 1d : 0.42d,
                            SelectionIndicatorVisibility =
                                SelectionMode == TimeSelectionMode.Day && date.IsSelected
                                    ? Visibility.Visible
                                    : Visibility.Collapsed,
                            TodayIndicatorVisibility = date.IsToday ? Visibility.Visible : Visibility.Collapsed
                        })
                        .ToArray()
                })
                .ToArray();

            StatusMessage = SelectionMode switch
            {
                TimeSelectionMode.Day => "Select a date and confirm the single-day period.",
                TimeSelectionMode.Week => "Select a week row or any date in that row and confirm the mapped week.",
                TimeSelectionMode.Month => "Select the month cell or any date and confirm the mapped month.",
                _ => "Time selection ready."
            };
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static IReadOnlyList<string> BuildWeekdayHeaders(TimeSelectionSnapshot snapshot)
    {
        var firstWeek = snapshot.Weeks.FirstOrDefault();
        if (firstWeek is null)
        {
            return ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        }

        return firstWeek.Dates
            .Select(cell => cell.Date.ToDateTime(TimeOnly.MinValue).ToString("ddd", CultureInfo.CurrentCulture))
            .ToArray();
    }
}
