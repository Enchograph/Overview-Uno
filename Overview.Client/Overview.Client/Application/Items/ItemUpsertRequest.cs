using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Items;

public sealed record ItemUpsertRequest
{
    public ItemType Type { get; init; } = ItemType.Task;

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Location { get; init; }

    public string? Color { get; init; }

    public bool IsImportant { get; init; }

    public bool IsCompleted { get; init; }

    public ReminderConfig ReminderConfig { get; init; } = new();

    public RepeatRule RepeatRule { get; init; } = new();

    public string? TimeZoneId { get; init; }

    public DateTimeOffset? StartAt { get; init; }

    public DateTimeOffset? EndAt { get; init; }

    public DateTimeOffset? PlannedStartAt { get; init; }

    public DateTimeOffset? PlannedEndAt { get; init; }

    public DateTimeOffset? DeadlineAt { get; init; }

    public int? ExpectedDurationMinutes { get; init; }

    public DateOnly? TargetDate { get; init; }
}
