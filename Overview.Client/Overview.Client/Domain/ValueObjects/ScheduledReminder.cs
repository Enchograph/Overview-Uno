using Overview.Client.Domain.Enums;

namespace Overview.Client.Domain.ValueObjects;

public sealed record ScheduledReminder
{
    public ReminderChannel Channel { get; init; } = ReminderChannel.Notification;

    public int MinutesBeforeStart { get; init; }

    public DateTimeOffset TriggerAt { get; init; }

    public DateTimeOffset OccurrenceStartAt { get; init; }

    public DateTimeOffset? OccurrenceEndAt { get; init; }
}
