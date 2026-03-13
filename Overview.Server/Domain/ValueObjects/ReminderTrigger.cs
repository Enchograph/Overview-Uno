using Overview.Server.Domain.Enums;

namespace Overview.Server.Domain.ValueObjects;

public sealed record ReminderTrigger
{
    public ReminderChannel Channel { get; init; } = ReminderChannel.Notification;

    public int MinutesBeforeStart { get; init; }
}
