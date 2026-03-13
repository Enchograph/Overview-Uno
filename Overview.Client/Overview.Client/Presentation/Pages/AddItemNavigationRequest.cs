namespace Overview.Client.Presentation.Pages;

public sealed record AddItemNavigationRequest
{
    public Guid? EditItemId { get; init; }

    public DateOnly? SuggestedStartDate { get; init; }

    public TimeOnly? SuggestedStartTime { get; init; }
}
