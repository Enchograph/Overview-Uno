using System.Globalization;
using Overview.Server.Domain.Entities;
using Overview.Server.Domain.Enums;
using Overview.Server.Domain.ValueObjects;

namespace Overview.Server.Domain.Rules;

public sealed class TimeRuleService : ITimeRuleService
{
    public IReadOnlyList<TimeBlockDefinition> BuildTimeBlocks(UserSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.TimeBlockCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Time block count must be greater than zero.");
        }

        if (settings.TimeBlockDurationMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Time block duration must be greater than zero.");
        }

        if (settings.TimeBlockGapMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Time block gap cannot be negative.");
        }

        var blocks = new List<TimeBlockDefinition>(settings.TimeBlockCount);

        for (var index = 0; index < settings.TimeBlockCount; index++)
        {
            var offsetMinutes = index * (settings.TimeBlockDurationMinutes + settings.TimeBlockGapMinutes);
            var startTime = settings.DayPlanStartTime.AddMinutes(offsetMinutes);
            var endTime = startTime.AddMinutes(settings.TimeBlockDurationMinutes);

            blocks.Add(new TimeBlockDefinition
            {
                Index = index,
                StartTime = startTime,
                EndTime = endTime,
                DurationMinutes = settings.TimeBlockDurationMinutes,
                GapMinutes = settings.TimeBlockGapMinutes,
            });
        }

        return blocks;
    }

    public CalendarPeriod GetPeriod(DateOnly referenceDate, TimeSelectionMode mode, DayOfWeek weekStartDay)
    {
        return mode switch
        {
            TimeSelectionMode.Day => new CalendarPeriod
            {
                Mode = mode,
                ReferenceDate = referenceDate,
                StartDate = referenceDate,
                EndDate = referenceDate,
            },
            TimeSelectionMode.Week => BuildWeekPeriod(referenceDate, weekStartDay),
            TimeSelectionMode.Month => BuildMonthPeriod(referenceDate),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported time selection mode."),
        };
    }

    public CalendarPeriod GetPreviousPeriod(CalendarPeriod period, DayOfWeek weekStartDay)
    {
        ArgumentNullException.ThrowIfNull(period);

        var previousReferenceDate = period.Mode switch
        {
            TimeSelectionMode.Day => period.ReferenceDate.AddDays(-1),
            TimeSelectionMode.Week => period.StartDate.AddDays(-1),
            TimeSelectionMode.Month => period.StartDate.AddDays(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(period), period.Mode, "Unsupported time selection mode."),
        };

        return GetPeriod(previousReferenceDate, period.Mode, weekStartDay);
    }

    public CalendarPeriod GetNextPeriod(CalendarPeriod period, DayOfWeek weekStartDay)
    {
        ArgumentNullException.ThrowIfNull(period);

        var nextReferenceDate = period.Mode switch
        {
            TimeSelectionMode.Day => period.ReferenceDate.AddDays(1),
            TimeSelectionMode.Week => period.EndDate.AddDays(1),
            TimeSelectionMode.Month => period.EndDate.AddDays(1),
            _ => throw new ArgumentOutOfRangeException(nameof(period), period.Mode, "Unsupported time selection mode."),
        };

        return GetPeriod(nextReferenceDate, period.Mode, weekStartDay);
    }

    public string FormatPeriodTitle(CalendarPeriod period, DayOfWeek weekStartDay, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(period);

        culture ??= CultureInfo.InvariantCulture;

        return period.Mode switch
        {
            TimeSelectionMode.Day => FormatDayTitle(period.ReferenceDate, culture),
            TimeSelectionMode.Week => FormatWeekTitle(period, weekStartDay, culture),
            TimeSelectionMode.Month => FormatMonthTitle(period.ReferenceDate, culture),
            _ => throw new ArgumentOutOfRangeException(nameof(period), period.Mode, "Unsupported time selection mode."),
        };
    }

    private static CalendarPeriod BuildWeekPeriod(DateOnly referenceDate, DayOfWeek weekStartDay)
    {
        var startDate = referenceDate.AddDays(-GetDayOffset(referenceDate.DayOfWeek, weekStartDay));
        var endDate = startDate.AddDays(6);

        return new CalendarPeriod
        {
            Mode = TimeSelectionMode.Week,
            ReferenceDate = referenceDate,
            StartDate = startDate,
            EndDate = endDate,
        };
    }

    private static CalendarPeriod BuildMonthPeriod(DateOnly referenceDate)
    {
        var startDate = new DateOnly(referenceDate.Year, referenceDate.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return new CalendarPeriod
        {
            Mode = TimeSelectionMode.Month,
            ReferenceDate = referenceDate,
            StartDate = startDate,
            EndDate = endDate,
        };
    }

    private static int GetDayOffset(DayOfWeek day, DayOfWeek weekStartDay)
    {
        return ((7 + (int)day - (int)weekStartDay) % 7);
    }

    private static string FormatDayTitle(DateOnly date, CultureInfo culture)
    {
        return IsChineseCulture(culture)
            ? $"{date.Year}/{date.Month}/{date.Day}"
            : date.ToString("yyyy/MM/dd", culture);
    }

    private static string FormatMonthTitle(DateOnly date, CultureInfo culture)
    {
        return IsChineseCulture(culture)
            ? $"{date.Year}年{date.Month}月"
            : date.ToString("MMMM yyyy", culture);
    }

    private static string FormatWeekTitle(CalendarPeriod period, DayOfWeek weekStartDay, CultureInfo culture)
    {
        var sameMonth = period.StartDate.Month == period.EndDate.Month && period.StartDate.Year == period.EndDate.Year;

        if (sameMonth)
        {
            var weekOfMonth = GetWeekOfMonth(period.StartDate, weekStartDay);

            return IsChineseCulture(culture)
                ? $"{period.StartDate.Year}年{period.StartDate.Month}月 第{weekOfMonth}周"
                : $"{period.StartDate.ToString("MMMM yyyy", culture)} - Week {weekOfMonth}";
        }

        var weekYear = ISOWeek.GetYear(period.ReferenceDate.ToDateTime(TimeOnly.MinValue));
        var weekNumber = ISOWeek.GetWeekOfYear(period.ReferenceDate.ToDateTime(TimeOnly.MinValue));

        return IsChineseCulture(culture)
            ? $"{weekYear}年第{weekNumber}周"
            : $"Week {weekNumber}, {weekYear}";
    }

    private static int GetWeekOfMonth(DateOnly weekStartDate, DayOfWeek weekStartDay)
    {
        var monthStart = new DateOnly(weekStartDate.Year, weekStartDate.Month, 1);
        var firstWeekStart = monthStart.AddDays(-GetDayOffset(monthStart.DayOfWeek, weekStartDay));

        return ((weekStartDate.DayNumber - firstWeekStart.DayNumber) / 7) + 1;
    }

    private static bool IsChineseCulture(CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase);
    }
}
