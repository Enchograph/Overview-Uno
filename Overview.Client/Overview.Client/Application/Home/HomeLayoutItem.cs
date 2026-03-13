using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Home;

public sealed record HomeLayoutItem
{
    public required Guid ItemId { get; init; }

    public required DateOnly ColumnDate { get; init; }

    public required ItemType Type { get; init; }

    public required string Title { get; init; }

    public required DateTimeOffset VisibleStartAt { get; init; }

    public required DateTimeOffset VisibleEndAt { get; init; }

    public required double TopRatio { get; init; }

    public required double HeightRatio { get; init; }

    public required double Opacity { get; init; }

    public bool IsClippedAtStart { get; init; }

    public bool IsClippedAtEnd { get; init; }
}
