using System.Globalization;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Home;

public sealed class TimeSelectionService : ITimeSelectionService
{
    private readonly IUserSettingsService userSettingsService;
    private readonly ITimeRuleService timeRuleService;

    public TimeSelectionService(
        IUserSettingsService userSettingsService,
        ITimeRuleService timeRuleService)
    {
        this.userSettingsService = userSettingsService;
        this.timeRuleService = timeRuleService;
    }

    public async Task<TimeSelectionSnapshot> BuildMonthSnapshotAsync(
        Guid userId,
        DateOnly visibleMonth,
        TimeSelectionMode selectionMode,
        DateOnly? selectedDate = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var culture = ResolveCulture(settings);
        var monthPeriod = timeRuleService.GetPeriod(visibleMonth, TimeSelectionMode.Month, settings.WeekStartDay);
        var gridStart = monthPeriod.StartDate.AddDays(-GetDayOffset(monthPeriod.StartDate.DayOfWeek, settings.WeekStartDay));
        var gridEnd = monthPeriod.EndDate.AddDays(6 - GetDayOffset(monthPeriod.EndDate.DayOfWeek, settings.WeekStartDay));
        var selectedPeriod = selectedDate is null
            ? null
            : timeRuleService.GetPeriod(selectedDate.Value, selectionMode, settings.WeekStartDay);

        var weeks = new List<TimeSelectionWeekRow>();
        var rowIndex = 0;

        for (var weekStart = gridStart; weekStart <= gridEnd; weekStart = weekStart.AddDays(7))
        {
            var weekPeriod = timeRuleService.GetPeriod(weekStart, TimeSelectionMode.Week, settings.WeekStartDay);
            var dates = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var date = weekStart.AddDays(offset);
                    var mappedPeriod = timeRuleService.GetPeriod(date, selectionMode, settings.WeekStartDay);

                    return new TimeSelectionDateCell
                    {
                        Date = date,
                        DayNumber = date.Day,
                        IsInVisibleMonth = date.Month == monthPeriod.ReferenceDate.Month && date.Year == monthPeriod.ReferenceDate.Year,
                        IsToday = date == DateOnly.FromDateTime(DateTime.Now.Date),
                        IsSelected = selectedPeriod is not null && AreSamePeriod(mappedPeriod, selectedPeriod),
                        MappedPeriod = mappedPeriod
                    };
                })
                .ToArray();

            weeks.Add(new TimeSelectionWeekRow
            {
                RowIndex = rowIndex++,
                WeekPeriod = weekPeriod,
                IsSelected = selectedPeriod is not null && AreSamePeriod(weekPeriod, selectedPeriod),
                Dates = dates
            });
        }

        return new TimeSelectionSnapshot
        {
            SelectionMode = selectionMode,
            VisibleMonth = monthPeriod,
            HeaderLabel = monthPeriod.ReferenceDate.ToString(
                culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase) ? "yyyy/M" : "yyyy/MM",
                culture),
            SelectedPeriod = selectedPeriod,
            Weeks = weeks
        };
    }

    public async Task<CalendarPeriod> ResolveSelectionAsync(
        Guid userId,
        DateOnly selectedDate,
        TimeSelectionMode selectionMode,
        CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        return timeRuleService.GetPeriod(selectedDate, selectionMode, settings.WeekStartDay);
    }

    public async Task<CalendarPeriod> GetPreviousPeriodAsync(
        Guid userId,
        CalendarPeriod period,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);

        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        return timeRuleService.GetPreviousPeriod(period, settings.WeekStartDay);
    }

    public async Task<CalendarPeriod> GetNextPeriodAsync(
        Guid userId,
        CalendarPeriod period,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);

        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        return timeRuleService.GetNextPeriod(period, settings.WeekStartDay);
    }

    private static CultureInfo ResolveCulture(UserSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Language))
        {
            try
            {
                return CultureInfo.GetCultureInfo(settings.Language);
            }
            catch (CultureNotFoundException)
            {
            }
        }

        return CultureInfo.InvariantCulture;
    }

    private static int GetDayOffset(DayOfWeek day, DayOfWeek weekStartDay)
    {
        return ((7 + (int)day - (int)weekStartDay) % 7);
    }

    private static bool AreSamePeriod(CalendarPeriod left, CalendarPeriod right)
    {
        return left.Mode == right.Mode
            && left.StartDate == right.StartDate
            && left.EndDate == right.EndDate;
    }
}
