using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Home;

public sealed record TimeSelectionDateCell
{
    public required DateOnly Date { get; init; }

    public required int DayNumber { get; init; }

    public required bool IsInVisibleMonth { get; init; }

    public required bool IsToday { get; init; }

    public required bool IsSelected { get; init; }

    public required CalendarPeriod MappedPeriod { get; init; }
}
