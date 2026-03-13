using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Lists;

public sealed record ListPageQuery
{
    public ListPageTab? Tab { get; init; }

    public ListSortBy? SortBy { get; init; }

    public DateOnly? ReferenceDate { get; init; }
}
