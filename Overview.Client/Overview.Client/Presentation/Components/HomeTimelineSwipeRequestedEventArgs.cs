namespace Overview.Client.Presentation.Components;

public sealed class HomeTimelineSwipeRequestedEventArgs : EventArgs
{
    public HomeTimelineSwipeRequestedEventArgs(bool isPrevious)
    {
        IsPrevious = isPrevious;
    }

    public bool IsPrevious { get; }
}
