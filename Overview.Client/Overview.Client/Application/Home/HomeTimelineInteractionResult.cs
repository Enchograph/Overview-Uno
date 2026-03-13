namespace Overview.Client.Application.Home;

public sealed record HomeTimelineInteractionResult
{
    public static HomeTimelineInteractionResult OutsideGrid { get; } = new()
    {
        IsWithinGrid = false
    };

    public bool IsWithinGrid { get; init; }

    public DateOnly ColumnDate { get; init; }

    public TimeOnly CellStartTime { get; init; }

    public TimeOnly HitTime { get; init; }

    public Guid? HitItemId { get; init; }
}
