using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Presentation.ViewModels;

public sealed class HomePageViewModel
{
    public string Title => "Home";

    public string Description => "Time selection component scaffold for the upcoming home timeline.";

    public TimeSelectionMode CurrentSelectionMode { get; private set; } = TimeSelectionMode.Week;

    public string StatusMessage { get; private set; } =
        "Use the picker to validate day, week, and month selection rules before the timeline grid lands.";

    public string ConfirmedSelectionText { get; private set; } = "No period confirmed yet.";

    public void SetSelectionMode(TimeSelectionMode selectionMode)
    {
        CurrentSelectionMode = selectionMode;
        StatusMessage = selectionMode switch
        {
            TimeSelectionMode.Day => "Day mode: tap a date cell and confirm a single day.",
            TimeSelectionMode.Week => "Week mode: tap a week row or any date in that row and confirm the mapped week.",
            TimeSelectionMode.Month => "Month mode: tap the month cell or any date and confirm the mapped month.",
            _ => StatusMessage
        };
    }

    public void ApplyConfirmedPeriod(CalendarPeriod period)
    {
        ConfirmedSelectionText =
            $"Confirmed {period.Mode}: {period.StartDate:yyyy-MM-dd} -> {period.EndDate:yyyy-MM-dd}";
        StatusMessage = "Selection confirmed.";
    }
}
