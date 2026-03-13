using Overview.Server.Domain.Enums;

namespace Overview.Server.Domain.ValueObjects;

public sealed record ScheduledReminder
{
    public ReminderChannel Channel { get; init; } = ReminderChannel.Notification;

    public int MinutesBeforeStart { get; init; }

    public DateTimeOffset TriggerAt { get; init; }

    public DateTimeOffset OccurrenceStartAt { get; init; }

    public DateTimeOffset? OccurrenceEndAt { get; init; }
}
