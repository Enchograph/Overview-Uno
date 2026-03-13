namespace Overview.Client.Presentation.ViewModels;

public sealed record ListPageThemeOptionViewModel
{
    public string ThemeKey { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public bool IsSelected { get; init; }
}
