namespace Overview.Client.Domain.ValueObjects;

public sealed record TimelineItemOverlap
{
    public Guid ItemId { get; init; }

    public int MaxConcurrentCount { get; init; }

    public double Opacity { get; init; }
}
