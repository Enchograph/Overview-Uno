using System.Globalization;
using Overview.Client.Application.Items;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Home;

public sealed class HomeLayoutService : IHomeLayoutService
{
    private readonly IItemService itemService;
    private readonly IUserSettingsService userSettingsService;
    private readonly ITimeRuleService timeRuleService;
    private readonly IHomeInteractionRuleService homeInteractionRuleService;

    public HomeLayoutService(
        IItemService itemService,
        IUserSettingsService userSettingsService,
        ITimeRuleService timeRuleService,
        IHomeInteractionRuleService homeInteractionRuleService)
    {
        this.itemService = itemService;
        this.userSettingsService = userSettingsService;
        this.timeRuleService = timeRuleService;
        this.homeInteractionRuleService = homeInteractionRuleService;
    }

    public async Task<HomeLayoutSnapshot> BuildSnapshotAsync(
        Guid userId,
        DateOnly referenceDate,
        HomeViewMode? viewMode = null,
        CultureInfo? culture = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var resolvedViewMode = viewMode ?? settings.HomeViewMode;
        var selectionMode = resolvedViewMode == HomeViewMode.Month ? TimeSelectionMode.Month : TimeSelectionMode.Week;
        var period = timeRuleService.GetPeriod(referenceDate, selectionMode, settings.WeekStartDay);
        var resolvedCulture = ResolveCulture(settings, culture);
        var timeBlocks = timeRuleService.BuildTimeBlocks(settings);
        var totalVisibleMinutes = GetTotalVisibleMinutes(timeBlocks);
        var columns = BuildColumns(period, resolvedCulture).ToArray();

        var items = await itemService.ListAsync(userId, cancellationToken: cancellationToken).ConfigureAwait(false);
        var segments = BuildSegments(items, period, timeBlocks, totalVisibleMinutes);
        var opacities = BuildOpacities(segments);

        var layoutItems = segments
            .Select(segment => new HomeLayoutItem
            {
                ItemId = segment.Item.Id,
                ColumnDate = segment.ColumnDate,
                Type = segment.Item.Type,
                Title = segment.Item.Title,
                VisibleStartAt = segment.VisibleStartAt,
                VisibleEndAt = segment.VisibleEndAt,
                TopRatio = segment.TopRatio,
                HeightRatio = segment.HeightRatio,
                Opacity = opacities.GetValueOrDefault(segment.Item.Id, 1d),
                IsClippedAtStart = segment.IsClippedAtStart,
                IsClippedAtEnd = segment.IsClippedAtEnd
            })
            .OrderBy(layoutItem => layoutItem.ColumnDate)
            .ThenBy(layoutItem => layoutItem.VisibleStartAt)
            .ThenBy(layoutItem => layoutItem.ItemId)
            .ToArray();

        return new HomeLayoutSnapshot
        {
            ViewMode = resolvedViewMode,
            Period = period,
            Title = timeRuleService.FormatPeriodTitle(period, settings.WeekStartDay, resolvedCulture),
            Columns = columns,
            TimeBlocks = timeBlocks,
            Items = layoutItems,
            TotalVisibleMinutes = totalVisibleMinutes
        };
    }

    private IReadOnlyList<HomeLayoutSegment> BuildSegments(
        IReadOnlyList<Item> items,
        CalendarPeriod period,
        IReadOnlyList<TimeBlockDefinition> timeBlocks,
        int totalVisibleMinutes)
    {
        var segments = new List<HomeLayoutSegment>();
        var planStart = timeBlocks[0].StartTime;
        var planEnd = timeBlocks[^1].EndTime;

        foreach (var item in items)
        {
            if (!TryGetDisplayRange(item, out var itemStart, out var itemEnd))
            {
                continue;
            }

            var firstDate = DateOnly.FromDateTime(itemStart.LocalDateTime.Date);
            var lastDate = DateOnly.FromDateTime(itemEnd.AddTicks(-1).LocalDateTime.Date);
            if (lastDate < period.StartDate || firstDate > period.EndDate)
            {
                continue;
            }

            var visibleStartDate = firstDate < period.StartDate ? period.StartDate : firstDate;
            var visibleEndDate = lastDate > period.EndDate ? period.EndDate : lastDate;

            for (var columnDate = visibleStartDate; columnDate <= visibleEndDate; columnDate = columnDate.AddDays(1))
            {
                var dayStart = new DateTimeOffset(columnDate.ToDateTime(TimeOnly.MinValue), itemStart.Offset);
                var dayEnd = dayStart.AddDays(1);
                var visibleStart = itemStart > dayStart ? itemStart : dayStart;
                var visibleEnd = itemEnd < dayEnd ? itemEnd : dayEnd;

                if (visibleEnd <= visibleStart)
                {
                    continue;
                }

                var planVisibleStart = new DateTimeOffset(columnDate.ToDateTime(planStart), itemStart.Offset);
                var planVisibleEnd = new DateTimeOffset(columnDate.ToDateTime(planEnd), itemStart.Offset);
                var clippedStart = visibleStart > planVisibleStart ? visibleStart : planVisibleStart;
                var clippedEnd = visibleEnd < planVisibleEnd ? visibleEnd : planVisibleEnd;

                if (clippedEnd <= clippedStart)
                {
                    continue;
                }

                var topRatio = ((clippedStart - planVisibleStart).TotalMinutes) / totalVisibleMinutes;
                var heightRatio = ((clippedEnd - clippedStart).TotalMinutes) / totalVisibleMinutes;

                segments.Add(new HomeLayoutSegment(
                    item,
                    columnDate,
                    visibleStart,
                    visibleEnd,
                    clippedStart,
                    clippedEnd,
                    Clamp01(topRatio),
                    Clamp01(heightRatio),
                    clippedStart > visibleStart,
                    clippedEnd < visibleEnd));
            }
        }

        return segments;
    }

    private Dictionary<Guid, double> BuildOpacities(IReadOnlyList<HomeLayoutSegment> segments)
    {
        if (segments.Count == 0)
        {
            return new Dictionary<Guid, double>();
        }

        var overlaps = homeInteractionRuleService.CalculateOverlapStates(
            segments.Select(segment => new TimelineItem
            {
                ItemId = segment.Item.Id,
                StartAt = segment.VisibleStartAt,
                EndAt = segment.VisibleEndAt
            }).ToArray());

        return overlaps
            .GroupBy(overlap => overlap.ItemId)
            .ToDictionary(
                group => group.Key,
                group => group.Min(overlap => overlap.Opacity));
    }

    private static IEnumerable<HomeDateColumn> BuildColumns(CalendarPeriod period, CultureInfo culture)
    {
        var today = DateOnly.FromDateTime(DateTime.Now.Date);

        for (var date = period.StartDate; date <= period.EndDate; date = date.AddDays(1))
        {
            yield return new HomeDateColumn
            {
                Date = date,
                HeaderLabel = FormatColumnLabel(date, culture),
                IsToday = date == today
            };
        }
    }

    private static bool TryGetDisplayRange(Item item, out DateTimeOffset startAt, out DateTimeOffset endAt)
    {
        if (item.Type == ItemType.Schedule && item.StartAt is not null && item.EndAt is not null)
        {
            startAt = item.StartAt.Value;
            endAt = item.EndAt.Value;
            return endAt > startAt;
        }

        if (item.Type == ItemType.Task && item.PlannedStartAt is not null && item.PlannedEndAt is not null)
        {
            startAt = item.PlannedStartAt.Value;
            endAt = item.PlannedEndAt.Value;
            return endAt > startAt;
        }

        startAt = default;
        endAt = default;
        return false;
    }

    private static int GetTotalVisibleMinutes(IReadOnlyList<TimeBlockDefinition> timeBlocks)
    {
        if (timeBlocks.Count == 0)
        {
            throw new InvalidOperationException("At least one time block is required.");
        }

        return (int)(timeBlocks[^1].EndTime - timeBlocks[0].StartTime).TotalMinutes;
    }

    private static string FormatColumnLabel(DateOnly date, CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase)
            ? $"{culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek)} {date.Month}/{date.Day}"
            : date.ToString("ddd M/d", culture);
    }

    private static CultureInfo ResolveCulture(UserSettings settings, CultureInfo? culture)
    {
        if (culture is not null)
        {
            return culture;
        }

        if (!string.IsNullOrWhiteSpace(settings.Language))
        {
            try
            {
                return CultureInfo.GetCultureInfo(settings.Language);
            }
            catch (CultureNotFoundException)
            {
            }
        }

        return CultureInfo.InvariantCulture;
    }

    private static double Clamp01(double value)
    {
        if (value < 0d)
        {
            return 0d;
        }

        if (value > 1d)
        {
            return 1d;
        }

        return value;
    }

    private sealed record HomeLayoutSegment(
        Item Item,
        DateOnly ColumnDate,
        DateTimeOffset VisibleStartAt,
        DateTimeOffset VisibleEndAt,
        DateTimeOffset ClippedStartAt,
        DateTimeOffset ClippedEndAt,
        double TopRatio,
        double HeightRatio,
        bool IsClippedAtStart,
        bool IsClippedAtEnd);
}
