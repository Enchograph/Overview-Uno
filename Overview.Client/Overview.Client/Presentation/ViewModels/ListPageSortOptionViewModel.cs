using Overview.Client.Domain.Enums;

namespace Overview.Client.Presentation.ViewModels;

public sealed record ListPageSortOptionViewModel
{
    public ListSortBy SortBy { get; init; }

    public string Label { get; init; } = string.Empty;

    public bool IsSelected { get; init; }
}
