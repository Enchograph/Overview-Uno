using System.Globalization;
using Overview.Server.Domain.Entities;
using Overview.Server.Domain.Enums;
using Overview.Server.Domain.ValueObjects;

namespace Overview.Server.Domain.Rules;

public interface ITimeRuleService
{
    IReadOnlyList<TimeBlockDefinition> BuildTimeBlocks(UserSettings settings);

    CalendarPeriod GetPeriod(DateOnly referenceDate, TimeSelectionMode mode, DayOfWeek weekStartDay);

    CalendarPeriod GetPreviousPeriod(CalendarPeriod period, DayOfWeek weekStartDay);

    CalendarPeriod GetNextPeriod(CalendarPeriod period, DayOfWeek weekStartDay);

    string FormatPeriodTitle(CalendarPeriod period, DayOfWeek weekStartDay, CultureInfo? culture = null);
}
