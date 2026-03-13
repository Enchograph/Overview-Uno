using System.Globalization;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Domain.Rules;

public interface ITimeRuleService
{
    IReadOnlyList<TimeBlockDefinition> BuildTimeBlocks(UserSettings settings);

    CalendarPeriod GetPeriod(DateOnly referenceDate, TimeSelectionMode mode, DayOfWeek weekStartDay);

    CalendarPeriod GetPreviousPeriod(CalendarPeriod period, DayOfWeek weekStartDay);

    CalendarPeriod GetNextPeriod(CalendarPeriod period, DayOfWeek weekStartDay);

    string FormatPeriodTitle(CalendarPeriod period, DayOfWeek weekStartDay, CultureInfo? culture = null);
}
