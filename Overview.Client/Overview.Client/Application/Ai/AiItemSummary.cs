namespace Overview.Client.Application.Ai;

public sealed record AiItemSummary
{
    public Guid Id { get; init; }

    public string ItemType { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Location { get; init; }

    public bool IsImportant { get; init; }

    public bool IsCompleted { get; init; }

    public string TimeSummary { get; init; } = string.Empty;

    public DateTimeOffset? StartAt { get; init; }

    public DateTimeOffset? EndAt { get; init; }

    public DateTimeOffset? DeadlineAt { get; init; }

    public int? ExpectedDurationMinutes { get; init; }

    public DateOnly? TargetDate { get; init; }
}
