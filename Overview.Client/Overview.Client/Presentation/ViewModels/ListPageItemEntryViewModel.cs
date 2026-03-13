namespace Overview.Client.Presentation.ViewModels;

public sealed record ListPageItemEntryViewModel
{
    public Guid ItemId { get; init; }

    public bool IsCompleted { get; init; }

    public bool IsImportant { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Subtitle { get; init; } = string.Empty;

    public string TypeBadge { get; init; } = string.Empty;

    public string ImportanceBadge { get; init; } = string.Empty;

    public string CompletionBadge { get; init; } = string.Empty;

    public string CompletionGlyph { get; init; } = string.Empty;

    public string ImportanceGlyph { get; init; } = string.Empty;
}
