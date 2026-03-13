using Overview.Client.Application.Home;

namespace Overview.Client.Presentation.Components;

public sealed class HomeTimelineInteractionRequestedEventArgs : EventArgs
{
    public HomeTimelineInteractionRequestedEventArgs(
        HomeTimelineInteractionResult interaction,
        bool isHold)
    {
        Interaction = interaction;
        IsHold = isHold;
    }

    public HomeTimelineInteractionResult Interaction { get; }

    public bool IsHold { get; }
}
