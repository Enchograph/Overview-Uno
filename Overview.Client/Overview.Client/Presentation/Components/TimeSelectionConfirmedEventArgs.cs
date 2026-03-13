using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Presentation.Components;

public sealed class TimeSelectionConfirmedEventArgs : EventArgs
{
    public TimeSelectionConfirmedEventArgs(CalendarPeriod selectedPeriod)
    {
        SelectedPeriod = selectedPeriod;
    }

    public CalendarPeriod SelectedPeriod { get; }
}
