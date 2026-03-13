namespace Overview.Server.Domain.ValueObjects;

public sealed record TimeBlockDefinition
{
    public required int Index { get; init; }

    public required TimeOnly StartTime { get; init; }

    public required TimeOnly EndTime { get; init; }

    public required int DurationMinutes { get; init; }

    public required int GapMinutes { get; init; }

    public string Label => $"{StartTime:HH\\:mm}~{EndTime:HH\\:mm}";
}
