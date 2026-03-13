using Overview.Server.Domain.Enums;
using Overview.Server.Domain.ValueObjects;

namespace Overview.Server.Domain.Entities;

public sealed class Item
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid UserId { get; init; }

    public ItemType Type { get; init; } = ItemType.Task;

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Location { get; init; }

    public string? Color { get; init; }

    public bool IsImportant { get; init; }

    public bool IsCompleted { get; init; }

    public ReminderConfig ReminderConfig { get; init; } = new();

    public RepeatRule RepeatRule { get; init; } = new();

    public string TimeZoneId { get; init; } = "UTC";

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public DateTimeOffset LastModifiedAt { get; init; }

    public string SourceDeviceId { get; init; } = string.Empty;

    public DateTimeOffset? StartAt { get; init; }

    public DateTimeOffset? EndAt { get; init; }

    public DateTimeOffset? PlannedStartAt { get; init; }

    public DateTimeOffset? PlannedEndAt { get; init; }

    public DateTimeOffset? DeadlineAt { get; init; }

    public int? ExpectedDurationMinutes { get; init; }

    public DateOnly? TargetDate { get; init; }
}
