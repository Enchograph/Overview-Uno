namespace Overview.Client.Domain.ValueObjects;

public sealed record ItemOccurrence
{
    public DateTimeOffset StartAt { get; init; }

    public DateTimeOffset? EndAt { get; init; }
}
