namespace Overview.Client.Domain.ValueObjects;

public sealed record WidgetPreferences
{
    public bool EnableHomeWidget { get; init; } = true;

    public bool EnableListWidget { get; init; } = true;

    public bool EnableAiShortcutWidget { get; init; } = true;

    public bool EnableQuickAddWidget { get; init; } = true;

    public bool ShowCompletedItemsInListWidget { get; init; }
}
