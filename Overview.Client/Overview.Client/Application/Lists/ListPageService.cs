using Overview.Client.Application.Items;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Lists;

public sealed class ListPageService : IListPageService
{
    private readonly IItemService itemService;
    private readonly IUserSettingsService userSettingsService;

    public ListPageService(
        IItemService itemService,
        IUserSettingsService userSettingsService)
    {
        this.itemService = itemService;
        this.userSettingsService = userSettingsService;
    }

    public async Task<ListPageSnapshot> BuildSnapshotAsync(
        Guid userId,
        ListPageQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var resolvedTab = query?.Tab ?? settings.ListPageDefaultTab;
        var resolvedSortBy = query?.SortBy ?? settings.ListPageSortBy;
        var timeZone = ResolveTimeZone(settings.TimeZoneId);
        var referenceDate = query?.ReferenceDate ?? GetToday(timeZone);
        var filteredItems = await GetFilteredItemsAsync(userId, resolvedTab, referenceDate, timeZone, cancellationToken)
            .ConfigureAwait(false);
        var orderMap = BuildManualOrderMap(settings.ListManualOrder, resolvedTab);

        var orderedItems = OrderItems(filteredItems, resolvedSortBy, referenceDate, timeZone, orderMap)
            .Select(item => ToListPageItem(item, timeZone))
            .ToArray();

        return new ListPageSnapshot
        {
            Tab = resolvedTab,
            SortBy = resolvedSortBy,
            ReferenceDate = referenceDate,
            Theme = settings.ListPageTheme,
            ActiveItems = orderedItems.Where(item => !item.IsCompleted).ToArray(),
            CompletedItems = orderedItems.Where(item => item.IsCompleted).ToArray()
        };
    }

    public async Task<UserSettings> SetSortByAsync(
        Guid userId,
        ListSortBy sortBy,
        CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        return await userSettingsService.SaveAsync(
            userId,
            ToUpdateRequest(settings) with
            {
                ListPageSortBy = sortBy
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserSettings> ReorderAsync(
        Guid userId,
        ListPageTab tab,
        IReadOnlyList<Guid> orderedItemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderedItemIds);

        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var filteredItems = await GetFilteredItemsAsync(
            userId,
            tab,
            GetToday(ResolveTimeZone(settings.TimeZoneId)),
            ResolveTimeZone(settings.TimeZoneId),
            cancellationToken).ConfigureAwait(false);

        var allowedIds = filteredItems.Select(item => item.Id).ToHashSet();
        var normalizedOrder = orderedItemIds
            .Where(allowedIds.Contains)
            .Concat(filteredItems.Select(item => item.Id))
            .Distinct()
            .ToArray();

        return await userSettingsService.SaveAsync(
            userId,
            ToUpdateRequest(settings) with
            {
                ListManualOrder = ApplyManualOrder(settings.ListManualOrder, tab, normalizedOrder)
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserSettings> SetThemeAsync(
        Guid userId,
        string theme,
        CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        return await userSettingsService.SaveAsync(
            userId,
            ToUpdateRequest(settings) with
            {
                ListPageTheme = NormalizeTheme(theme)
            },
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<Item>> GetFilteredItemsAsync(
        Guid userId,
        ListPageTab tab,
        DateOnly referenceDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken)
    {
        var items = await itemService.ListAsync(userId, cancellationToken: cancellationToken).ConfigureAwait(false);
        return items.Where(item => MatchesTab(item, tab, referenceDate, timeZone)).ToArray();
    }

    private static bool MatchesTab(
        Item item,
        ListPageTab tab,
        DateOnly referenceDate,
        TimeZoneInfo timeZone)
    {
        return tab switch
        {
            ListPageTab.MyDay => IsRelevantOnDate(item, referenceDate, timeZone),
            ListPageTab.AllItems => true,
            ListPageTab.Tasks => item.Type == ItemType.Task,
            ListPageTab.Schedules => item.Type == ItemType.Schedule,
            ListPageTab.Notes => item.Type == ItemType.Note,
            ListPageTab.Important => item.IsImportant,
            _ => true
        };
    }

    private static bool IsRelevantOnDate(
        Item item,
        DateOnly referenceDate,
        TimeZoneInfo timeZone)
    {
        if (TryGetRelevantRange(item, timeZone, out var startDate, out var endDate))
        {
            return startDate <= referenceDate && endDate >= referenceDate;
        }

        if (item.DeadlineAt is not null)
        {
            return ConvertToDate(item.DeadlineAt.Value, timeZone) == referenceDate;
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

    private static int GetCompletionBucket(Item item) => item.IsCompleted ? 1 : 0;

    private static IOrderedEnumerable<Item> OrderItems(
        IReadOnlyList<Item> items,
        ListSortBy sortBy,
        DateOnly referenceDate,
        TimeZoneInfo timeZone,
        IReadOnlyDictionary<Guid, int> orderMap)
    {
        return sortBy switch
        {
            ListSortBy.Importance => items
                .OrderBy(item => GetCompletionBucket(item))
                .ThenBy(item => GetManualOrderRank(orderMap, item.Id))
                .ThenBy(item => item.IsImportant ? 0 : 1)
                .ThenBy(item => GetDateSortBucket(GetDueDate(item, timeZone)))
                .ThenBy(item => GetDueDate(item, timeZone))
                .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(item => item.CreatedAt)
                .ThenBy(item => item.Id),
            ListSortBy.DueDate => items
                .OrderBy(item => GetCompletionBucket(item))
                .ThenBy(item => GetManualOrderRank(orderMap, item.Id))
                .ThenBy(item => GetDateSortBucket(GetDueDate(item, timeZone)))
                .ThenBy(item => GetDueDate(item, timeZone))
                .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(item => item.CreatedAt)
                .ThenBy(item => item.Id),
            ListSortBy.TodayExecutor => items
                .OrderBy(item => GetCompletionBucket(item))
                .ThenBy(item => GetManualOrderRank(orderMap, item.Id))
                .ThenBy(item => IsRelevantOnDate(item, referenceDate, timeZone) ? 0 : 1)
                .ThenBy(item => GetDateSortBucket(GetDueDate(item, timeZone)))
                .ThenBy(item => GetDueDate(item, timeZone))
                .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(item => item.CreatedAt)
                .ThenBy(item => item.Id),
            ListSortBy.Alphabetical => items
                .OrderBy(item => GetCompletionBucket(item))
                .ThenBy(item => GetManualOrderRank(orderMap, item.Id))
                .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(item => item.CreatedAt)
                .ThenBy(item => item.Id),
            ListSortBy.CreatedAt => items
                .OrderBy(item => GetCompletionBucket(item))
                .ThenBy(item => GetManualOrderRank(orderMap, item.Id))
                .ThenByDescending(item => item.CreatedAt)
                .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(item => item.Id),
            _ => items
                .OrderBy(item => GetCompletionBucket(item))
                .ThenBy(item => GetManualOrderRank(orderMap, item.Id))
                .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(item => item.CreatedAt)
                .ThenBy(item => item.Id)
        };
    }

    private static int GetDateSortBucket(DateOnly? date)
    {
        return date is null ? 1 : 0;
    }

    private static DateOnly? GetDueDate(Item item, TimeZoneInfo timeZone)
    {
        if (item.DeadlineAt is not null)
        {
            return ConvertToDate(item.DeadlineAt.Value, timeZone);
        }

        if (item.Type == ItemType.Schedule && item.StartAt is not null)
        {
            return ConvertToDate(item.StartAt.Value, timeZone);
        }

        if (item.Type == ItemType.Task && item.PlannedStartAt is not null)
        {
            return ConvertToDate(item.PlannedStartAt.Value, timeZone);
        }

        return item.TargetDate;
    }

    private static ListPageItem ToListPageItem(Item item, TimeZoneInfo timeZone)
    {
        return new ListPageItem
        {
            ItemId = item.Id,
            Type = item.Type,
            Title = item.Title,
            IsImportant = item.IsImportant,
            IsCompleted = item.IsCompleted,
            RelevantDate = GetRelevantDate(item, timeZone),
            DeadlineDate = item.DeadlineAt is null ? null : ConvertToDate(item.DeadlineAt.Value, timeZone),
            LastModifiedAt = item.LastModifiedAt
        };
    }

    private static DateOnly? GetRelevantDate(Item item, TimeZoneInfo timeZone)
    {
        if (item.StartAt is not null)
        {
            return ConvertToDate(item.StartAt.Value, timeZone);
        }

        if (item.PlannedStartAt is not null)
        {
            return ConvertToDate(item.PlannedStartAt.Value, timeZone);
        }

        if (item.DeadlineAt is not null)
        {
            return ConvertToDate(item.DeadlineAt.Value, timeZone);
        }

        return item.TargetDate;
    }

    private static Dictionary<Guid, int> BuildManualOrderMap(
        ListManualOrderPreferences preferences,
        ListPageTab tab)
    {
        return GetManualOrder(preferences, tab)
            .Distinct()
            .Select((itemId, index) => new { itemId, index })
            .ToDictionary(entry => entry.itemId, entry => entry.index);
    }

    private static int GetManualOrderRank(IReadOnlyDictionary<Guid, int> orderMap, Guid itemId)
    {
        return orderMap.TryGetValue(itemId, out var index) ? index : int.MaxValue;
    }

    private static IReadOnlyList<Guid> GetManualOrder(ListManualOrderPreferences preferences, ListPageTab tab)
    {
        return tab switch
        {
            ListPageTab.MyDay => preferences.MyDay,
            ListPageTab.AllItems => preferences.AllItems,
            ListPageTab.Tasks => preferences.Tasks,
            ListPageTab.Schedules => preferences.Schedules,
            ListPageTab.Notes => preferences.Notes,
            ListPageTab.Important => preferences.Important,
            _ => Array.Empty<Guid>()
        };
    }

    private static ListManualOrderPreferences ApplyManualOrder(
        ListManualOrderPreferences preferences,
        ListPageTab tab,
        IReadOnlyList<Guid> orderedItemIds)
    {
        return tab switch
        {
            ListPageTab.MyDay => preferences with { MyDay = orderedItemIds.ToArray() },
            ListPageTab.AllItems => preferences with { AllItems = orderedItemIds.ToArray() },
            ListPageTab.Tasks => preferences with { Tasks = orderedItemIds.ToArray() },
            ListPageTab.Schedules => preferences with { Schedules = orderedItemIds.ToArray() },
            ListPageTab.Notes => preferences with { Notes = orderedItemIds.ToArray() },
            ListPageTab.Important => preferences with { Important = orderedItemIds.ToArray() },
            _ => preferences
        };
    }

    private static UserSettingsUpdateRequest ToUpdateRequest(UserSettings settings)
    {
        return new UserSettingsUpdateRequest
        {
            Language = settings.Language,
            ThemeMode = settings.ThemeMode,
            ThemePreset = settings.ThemePreset,
            WeekStartDay = settings.WeekStartDay,
            HomeViewMode = settings.HomeViewMode,
            DayPlanStartTime = settings.DayPlanStartTime,
            TimeBlockDurationMinutes = settings.TimeBlockDurationMinutes,
            TimeBlockGapMinutes = settings.TimeBlockGapMinutes,
            TimeBlockCount = settings.TimeBlockCount,
            ListPageDefaultTab = settings.ListPageDefaultTab,
            ListPageSortBy = settings.ListPageSortBy,
            ListPageTheme = settings.ListPageTheme,
            ListManualOrder = settings.ListManualOrder,
            AiBaseUrl = settings.AiBaseUrl,
            AiApiKey = settings.AiApiKey,
            AiModel = settings.AiModel,
            SyncServerBaseUrl = settings.SyncServerBaseUrl,
            NotificationEnabled = settings.NotificationEnabled,
            WidgetPreferences = settings.WidgetPreferences,
            TimeZoneId = settings.TimeZoneId
        };
    }

    private static DateOnly GetToday(TimeZoneInfo timeZone)
    {
        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        return DateOnly.FromDateTime(localNow.Date);
    }

    private static DateOnly ConvertToDate(DateTimeOffset value, TimeZoneInfo timeZone)
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, timeZone).Date);
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

    private static string NormalizeTheme(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme))
        {
            return "default";
        }

        return theme.Trim().ToLowerInvariant() switch
        {
            "sunrise" => "sunrise",
            "forest" => "forest",
            "slate" => "slate",
            _ => "default"
        };
    }
}
