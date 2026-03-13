using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Lists;

public sealed record ListPageSnapshot
{
    public ListPageTab Tab { get; init; }

    public ListSortBy SortBy { get; init; }

    public DateOnly ReferenceDate { get; init; }

    public string Theme { get; init; } = "default";

    public IReadOnlyList<ListPageItem> ActiveItems { get; init; } = Array.Empty<ListPageItem>();

    public IReadOnlyList<ListPageItem> CompletedItems { get; init; } = Array.Empty<ListPageItem>();
}
