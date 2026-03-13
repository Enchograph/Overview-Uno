using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Presentation.ViewModels;

public sealed record TimeSelectionWeekRowViewModel
{
    public required string WeekLabel { get; init; }

    public required CalendarPeriod WeekPeriod { get; init; }

    public required Visibility SelectionIndicatorVisibility { get; init; }

    public required IReadOnlyList<TimeSelectionDateCellViewModel> Dates { get; init; }
}
