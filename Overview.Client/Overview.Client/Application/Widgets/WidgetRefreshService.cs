using System.Globalization;
using Overview.Client.Application.Navigation;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;
using Overview.Client.Domain.ValueObjects;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Widgets;

namespace Overview.Client.Application.Widgets;

public sealed class WidgetRefreshService : IWidgetRefreshService
{
    private readonly IItemRepository itemRepository;
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly IAiChatMessageRepository aiChatMessageRepository;
    private readonly ITimeRuleService timeRuleService;
    private readonly IWidgetSnapshotStore widgetSnapshotStore;
    private readonly IWidgetRenderer widgetRenderer;
    private readonly TimeProvider timeProvider;

    public WidgetRefreshService(
        IItemRepository itemRepository,
        IUserSettingsRepository userSettingsRepository,
        IAiChatMessageRepository aiChatMessageRepository,
        ITimeRuleService timeRuleService,
        IWidgetSnapshotStore widgetSnapshotStore,
        IWidgetRenderer widgetRenderer,
        TimeProvider timeProvider)
    {
        this.itemRepository = itemRepository;
        this.userSettingsRepository = userSettingsRepository;
        this.aiChatMessageRepository = aiChatMessageRepository;
        this.timeRuleService = timeRuleService;
        this.widgetSnapshotStore = widgetSnapshotStore;
        this.widgetRenderer = widgetRenderer;
        this.timeProvider = timeProvider;
    }

    public async Task RefreshAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsRepository.GetAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? CreateFallbackSettings(userId);
        var items = await itemRepository.ListByUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var timeZone = ResolveTimeZone(settings.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).Date);
        var messages = await aiChatMessageRepository.ListByDateRangeAsync(userId, today, today, cancellationToken).ConfigureAwait(false);

        await RefreshWidgetAsync(
            settings.WidgetPreferences.EnableHomeWidget,
            BuildHomeSnapshot(settings, items),
            WidgetKind.Home,
            cancellationToken).ConfigureAwait(false);

        await RefreshWidgetAsync(
            settings.WidgetPreferences.EnableListWidget,
            BuildListSnapshot(settings, items),
            WidgetKind.List,
            cancellationToken).ConfigureAwait(false);

        await RefreshWidgetAsync(
            settings.WidgetPreferences.EnableAiShortcutWidget,
            BuildAiShortcutSnapshot(settings, messages),
            WidgetKind.AiShortcut,
            cancellationToken).ConfigureAwait(false);

        await RefreshWidgetAsync(
            settings.WidgetPreferences.EnableQuickAddWidget,
            BuildQuickAddSnapshot(),
            WidgetKind.QuickAdd,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _ = userId;

        foreach (var kind in Enum.GetValues<WidgetKind>())
        {
            await widgetSnapshotStore.RemoveAsync(kind, cancellationToken).ConfigureAwait(false);
            await widgetRenderer.ClearAsync(kind, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RefreshWidgetAsync(
        bool isEnabled,
        WidgetSnapshot snapshot,
        WidgetKind kind,
        CancellationToken cancellationToken)
    {
        if (!isEnabled)
        {
            await widgetSnapshotStore.RemoveAsync(kind, cancellationToken).ConfigureAwait(false);
            await widgetRenderer.ClearAsync(kind, cancellationToken).ConfigureAwait(false);
            return;
        }

        await widgetSnapshotStore.SaveAsync(snapshot, cancellationToken).ConfigureAwait(false);
        await widgetRenderer.RenderAsync(snapshot, cancellationToken).ConfigureAwait(false);
    }

    private WidgetSnapshot BuildHomeSnapshot(UserSettings settings, IReadOnlyList<Item> items)
    {
        var timeZone = ResolveTimeZone(settings.TimeZoneId);
        var culture = ResolveCulture(settings.Language);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).Date);
        var selectionMode = settings.HomeViewMode == HomeViewMode.Month ? TimeSelectionMode.Month : TimeSelectionMode.Week;
        var period = timeRuleService.GetPeriod(today, selectionMode, settings.WeekStartDay);
        var relevantItems = items
            .Where(item => item.DeletedAt is null && !item.IsCompleted)
            .Where(item => item.Type is ItemType.Schedule or ItemType.Task)
            .ToArray();

        var entries = EnumerateDates(period)
            .Select(date => BuildHomeEntry(date, relevantItems, culture, timeZone))
            .Take(4)
            .ToArray();

        return new WidgetSnapshot
        {
            Kind = WidgetKind.Home,
            Title = "Overview Home",
            Subtitle = timeRuleService.FormatPeriodTitle(period, settings.WeekStartDay, culture),
            DeepLink = AppNavigationRequest.CreateDeepLink(AppNavigationTarget.Home),
            GeneratedAt = timeProvider.GetUtcNow(),
            Entries = entries.Length == 0
                ? [CreateEmptyEntry("No timeline items", "Create a schedule or task to populate the home widget.")]
                : entries
        };
    }

    private static WidgetSnapshotEntry BuildHomeEntry(
        DateOnly date,
        IReadOnlyList<Item> items,
        CultureInfo culture,
        TimeZoneInfo timeZone)
    {
        var matchingItems = items
            .Where(item => IsVisibleOnDate(item, date, timeZone))
            .OrderBy(item => GetStartAt(item) ?? DateTimeOffset.MaxValue)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        var title = culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase)
            ? $"{culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek)} {date.Month}/{date.Day}"
            : date.ToString("ddd M/d", culture);

        if (matchingItems.Length == 0)
        {
            return new WidgetSnapshotEntry
            {
                Title = title,
                Subtitle = "No scheduled items",
                BadgeText = "0"
            };
        }

        var firstItem = matchingItems[0];
        return new WidgetSnapshotEntry
        {
            Title = title,
            Subtitle = TrimToLength(firstItem.Title, 34),
            BadgeText = matchingItems.Length.ToString(CultureInfo.InvariantCulture),
            AccentColor = firstItem.Color
        };
    }

    private WidgetSnapshot BuildListSnapshot(UserSettings settings, IReadOnlyList<Item> items)
    {
        var timeZone = ResolveTimeZone(settings.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).Date);
        var filteredItems = items
            .Where(item => item.DeletedAt is null)
            .Where(item => settings.WidgetPreferences.ShowCompletedItemsInListWidget || !item.IsCompleted)
            .Where(item => MatchesTab(item, settings.ListPageDefaultTab, today, timeZone))
            .OrderBy(item => item.IsCompleted ? 1 : 0)
            .ThenBy(item => item.IsImportant ? 0 : 1)
            .ThenBy(item => GetDueDate(item, timeZone) ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Take(4)
            .ToArray();

        return new WidgetSnapshot
        {
            Kind = WidgetKind.List,
            Title = "Overview List",
            Subtitle = $"{FormatListTab(settings.ListPageDefaultTab)} · {filteredItems.Count(item => !item.IsCompleted)} active",
            DeepLink = AppNavigationRequest.CreateDeepLink(AppNavigationTarget.List),
            GeneratedAt = timeProvider.GetUtcNow(),
            Entries = filteredItems.Length == 0
                ? [CreateEmptyEntry("No list items", "Your current tab is empty.")]
                : filteredItems.Select(BuildListEntry).ToArray()
        };
    }

    private static WidgetSnapshotEntry BuildListEntry(Item item)
    {
        return new WidgetSnapshotEntry
        {
            Title = TrimToLength(item.Title, 34),
            Subtitle = item.IsCompleted
                ? "Completed"
                : item.IsImportant
                    ? "Important"
                    : FormatItemKind(item.Type),
            BadgeText = item.IsCompleted ? "Done" : null,
            AccentColor = item.Color
        };
    }

    private WidgetSnapshot BuildAiShortcutSnapshot(UserSettings settings, IReadOnlyList<AiChatMessage> messages)
    {
        var timeZone = ResolveTimeZone(settings.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).Date);
        var recentMessages = messages
            .Where(message => message.OccurredOn == today)
            .OrderByDescending(message => message.CreatedAt)
            .Take(3)
            .OrderBy(message => message.CreatedAt)
            .ToArray();

        return new WidgetSnapshot
        {
            Kind = WidgetKind.AiShortcut,
            Title = "Overview AI",
            Subtitle = recentMessages.Length == 0
                ? "Ask AI to create, delete, or summarize your items."
                : $"Today: {recentMessages.Length} message(s)",
            DeepLink = AppNavigationRequest.CreateDeepLink(AppNavigationTarget.Ai),
            GeneratedAt = timeProvider.GetUtcNow(),
            Entries = recentMessages.Length == 0
                ? [CreateEmptyEntry("Open AI chat", "Tap to continue today’s planning conversation.")]
                : recentMessages.Select(message => new WidgetSnapshotEntry
                {
                    Title = message.Role == AiChatRole.Assistant ? "Assistant" : "You",
                    Subtitle = TrimToLength(message.Message, 40)
                }).ToArray()
        };
    }

    private WidgetSnapshot BuildQuickAddSnapshot()
    {
        return new WidgetSnapshot
        {
            Kind = WidgetKind.QuickAdd,
            Title = "Quick Add",
            Subtitle = "Create a new task, schedule, or note.",
            DeepLink = AppNavigationRequest.CreateDeepLink(AppNavigationTarget.Add, ItemType.Task),
            GeneratedAt = timeProvider.GetUtcNow(),
            Entries =
            [
                new WidgetSnapshotEntry
                {
                    Title = "New task",
                    Subtitle = "Open the add page with task defaults."
                }
            ]
        };
    }

    private static bool MatchesTab(Item item, ListPageTab tab, DateOnly referenceDate, TimeZoneInfo timeZone)
    {
        return tab switch
        {
            ListPageTab.MyDay => IsVisibleOnDate(item, referenceDate, timeZone),
            ListPageTab.AllItems => true,
            ListPageTab.Tasks => item.Type == ItemType.Task,
            ListPageTab.Schedules => item.Type == ItemType.Schedule,
            ListPageTab.Notes => item.Type == ItemType.Note,
            ListPageTab.Important => item.IsImportant,
            _ => true
        };
    }

    private static bool IsVisibleOnDate(Item item, DateOnly date, TimeZoneInfo timeZone)
    {
        if (TryGetRelevantRange(item, timeZone, out var startDate, out var endDate))
        {
            return startDate <= date && endDate >= date;
        }

        return false;
    }

    private static bool TryGetRelevantRange(
        Item item,
        TimeZoneInfo timeZone,
        out DateOnly startDate,
        out DateOnly endDate)
    {
        if (item.Type == ItemType.Schedule && item.StartAt is not null && item.EndAt is not null)
        {
            startDate = ConvertToDate(item.StartAt.Value, timeZone);
            endDate = ConvertToDate(item.EndAt.Value.AddTicks(-1), timeZone);
            return true;
        }

        if (item.Type == ItemType.Task)
        {
            if (item.PlannedStartAt is not null && item.PlannedEndAt is not null)
            {
                startDate = ConvertToDate(item.PlannedStartAt.Value, timeZone);
                endDate = ConvertToDate(item.PlannedEndAt.Value.AddTicks(-1), timeZone);
                return true;
            }

            if (item.DeadlineAt is not null)
            {
                startDate = ConvertToDate(item.DeadlineAt.Value, timeZone);
                endDate = startDate;
                return true;
            }
        }

        if (item.Type == ItemType.Note && item.TargetDate is not null)
        {
            startDate = item.TargetDate.Value;
            endDate = startDate;
            return true;
        }

        startDate = default;
        endDate = default;
        return false;
    }

    private static DateOnly? GetDueDate(Item item, TimeZoneInfo timeZone)
    {
        if (item.Type == ItemType.Task && item.DeadlineAt is not null)
        {
            return ConvertToDate(item.DeadlineAt.Value, timeZone);
        }

        if (item.Type == ItemType.Schedule && item.EndAt is not null)
        {
            return ConvertToDate(item.EndAt.Value, timeZone);
        }

        if (item.Type == ItemType.Note)
        {
            return item.TargetDate;
        }

        return null;
    }

    private static DateOnly ConvertToDate(DateTimeOffset timestamp, TimeZoneInfo timeZone)
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timestamp, timeZone).Date);
    }

    private static DateTimeOffset? GetStartAt(Item item)
    {
        return item.Type switch
        {
            ItemType.Schedule => item.StartAt,
            ItemType.Task => item.PlannedStartAt ?? item.DeadlineAt,
            _ => null
        };
    }

    private static string FormatListTab(ListPageTab tab)
    {
        return tab switch
        {
            ListPageTab.MyDay => "My Day",
            ListPageTab.AllItems => "All Items",
            ListPageTab.Tasks => "Tasks",
            ListPageTab.Schedules => "Schedules",
            ListPageTab.Notes => "Notes",
            ListPageTab.Important => "Important",
            _ => "List"
        };
    }

    private static string FormatItemKind(ItemType type)
    {
        return type switch
        {
            ItemType.Schedule => "Schedule",
            ItemType.Task => "Task",
            ItemType.Note => "Note",
            _ => "Item"
        };
    }

    private static WidgetSnapshotEntry CreateEmptyEntry(string title, string subtitle)
    {
        return new WidgetSnapshotEntry
        {
            Title = title,
            Subtitle = subtitle
        };
    }

    private static string TrimToLength(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return $"{value[..Math.Max(0, maxLength - 1)]}…";
    }

    private static CultureInfo ResolveCulture(string? language)
    {
        if (!string.IsNullOrWhiteSpace(language))
        {
            try
            {
                return CultureInfo.GetCultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
            }
        }

        return CultureInfo.InvariantCulture;
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }

    private static UserSettings CreateFallbackSettings(Guid userId)
    {
        return new UserSettings
        {
            UserId = userId,
            Language = "en-US",
            TimeZoneId = "UTC"
        };
    }

    private static IEnumerable<DateOnly> EnumerateDates(CalendarPeriod period)
    {
        for (var date = period.StartDate; date <= period.EndDate; date = date.AddDays(1))
        {
            yield return date;
        }
    }
}
