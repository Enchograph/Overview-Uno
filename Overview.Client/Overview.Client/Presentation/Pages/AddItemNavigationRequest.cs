namespace Overview.Client.Presentation.Pages;

public sealed record AddItemNavigationRequest
{
    public Guid? EditItemId { get; init; }

    public string? SourceTabKey { get; init; }

    public Domain.Enums.ItemType? SuggestedType { get; init; }

    public bool? SuggestedIsImportant { get; init; }

    public DateOnly? SuggestedStartDate { get; init; }

    public TimeOnly? SuggestedStartTime { get; init; }
}
