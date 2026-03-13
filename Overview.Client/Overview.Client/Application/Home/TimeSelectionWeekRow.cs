using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Home;

public sealed record TimeSelectionWeekRow
{
    public required int RowIndex { get; init; }

    public required CalendarPeriod WeekPeriod { get; init; }

    public required bool IsSelected { get; init; }

    public required IReadOnlyList<TimeSelectionDateCell> Dates { get; init; }
}
