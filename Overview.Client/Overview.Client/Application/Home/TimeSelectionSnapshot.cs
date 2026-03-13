using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Home;

public sealed record TimeSelectionSnapshot
{
    public required TimeSelectionMode SelectionMode { get; init; }

    public required CalendarPeriod VisibleMonth { get; init; }

    public required string HeaderLabel { get; init; }

    public required CalendarPeriod? SelectedPeriod { get; init; }

    public required IReadOnlyList<TimeSelectionWeekRow> Weeks { get; init; }
}
