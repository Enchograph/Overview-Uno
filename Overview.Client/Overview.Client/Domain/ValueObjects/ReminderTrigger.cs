using Overview.Client.Domain.Enums;

namespace Overview.Client.Domain.ValueObjects;

public sealed record ReminderTrigger
{
    public ReminderChannel Channel { get; init; } = ReminderChannel.Notification;

    public int MinutesBeforeStart { get; init; }
}
