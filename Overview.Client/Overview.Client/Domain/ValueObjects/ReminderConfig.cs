namespace Overview.Client.Domain.ValueObjects;

public sealed record ReminderConfig
{
    public bool IsEnabled { get; init; }

    public IReadOnlyList<ReminderTrigger> Triggers { get; init; } = Array.Empty<ReminderTrigger>();
}
