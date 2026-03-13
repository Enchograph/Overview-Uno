namespace Overview.Client.Presentation.ViewModels;

public sealed record TimeSelectionDateCellViewModel
{
    public required DateOnly Date { get; init; }

    public required string DayNumberText { get; init; }

    public required string CaptionText { get; init; }

    public required bool IsEnabled { get; init; }

    public required bool IsToday { get; init; }

    public required double CellOpacity { get; init; }

    public required Visibility SelectionIndicatorVisibility { get; init; }

    public required Visibility TodayIndicatorVisibility { get; init; }
}
