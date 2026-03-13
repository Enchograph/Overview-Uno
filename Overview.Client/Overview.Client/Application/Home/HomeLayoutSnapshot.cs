using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Home;

public sealed record HomeLayoutSnapshot
{
    public required HomeViewMode ViewMode { get; init; }

    public required CalendarPeriod Period { get; init; }

    public required string Title { get; init; }

    public required IReadOnlyList<HomeDateColumn> Columns { get; init; }

    public required IReadOnlyList<TimeBlockDefinition> TimeBlocks { get; init; }

    public required IReadOnlyList<HomeLayoutItem> Items { get; init; }

    public required int TotalVisibleMinutes { get; init; }
}
