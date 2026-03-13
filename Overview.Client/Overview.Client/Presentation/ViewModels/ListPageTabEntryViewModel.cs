using Overview.Client.Domain.Enums;

namespace Overview.Client.Presentation.ViewModels;

public sealed record ListPageTabEntryViewModel
{
    public string Key { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public ListPageTab Tab { get; init; }

    public bool IsSelected { get; init; }
}
