namespace Overview.Client.Application.Home;

public interface IHomeTimelineInteractionService
{
    HomeTimelineInteractionResult ResolveInteraction(
        HomeLayoutSnapshot snapshot,
        int columnIndex,
        double verticalPositionRatio);
}
