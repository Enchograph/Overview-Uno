namespace Overview.Client.Domain.ValueObjects;

public sealed record TimelineItem
{
    public Guid ItemId { get; init; }

    public DateTimeOffset StartAt { get; init; }

    public DateTimeOffset EndAt { get; init; }
}
