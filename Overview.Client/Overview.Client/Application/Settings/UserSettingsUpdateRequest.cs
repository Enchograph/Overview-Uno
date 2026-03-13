using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Settings;

public sealed record UserSettingsUpdateRequest
{
    public string Language { get; init; } = "zh-CN";

    public ThemeMode ThemeMode { get; init; } = ThemeMode.System;

    public string ThemePreset { get; init; } = "default";

    public DayOfWeek WeekStartDay { get; init; } = DayOfWeek.Monday;

    public HomeViewMode HomeViewMode { get; init; } = HomeViewMode.Week;

    public TimeOnly DayPlanStartTime { get; init; } = new(8, 0);

    public int TimeBlockDurationMinutes { get; init; } = 60;

    public int TimeBlockGapMinutes { get; init; }

    public int TimeBlockCount { get; init; } = 12;

    public ListPageTab ListPageDefaultTab { get; init; } = ListPageTab.MyDay;

    public ListSortBy ListPageSortBy { get; init; } = ListSortBy.Importance;

    public string ListPageTheme { get; init; } = "default";

    public string AiBaseUrl { get; init; } = string.Empty;

    public string AiApiKey { get; init; } = string.Empty;

    public string AiModel { get; init; } = string.Empty;

    public string SyncServerBaseUrl { get; init; } = string.Empty;

    public bool NotificationEnabled { get; init; } = true;

    public WidgetPreferences WidgetPreferences { get; init; } = new();

    public string? TimeZoneId { get; init; }
}
