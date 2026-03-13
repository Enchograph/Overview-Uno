using Overview.Client.Domain.Rules;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Home;

public sealed class HomeTimelineInteractionService : IHomeTimelineInteractionService
{
    private readonly IHomeInteractionRuleService homeInteractionRuleService;

    public HomeTimelineInteractionService(IHomeInteractionRuleService homeInteractionRuleService)
    {
        this.homeInteractionRuleService = homeInteractionRuleService;
    }

    public HomeTimelineInteractionResult ResolveInteraction(
        HomeLayoutSnapshot snapshot,
        int columnIndex,
        double verticalPositionRatio)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.Columns.Count == 0 || snapshot.TimeBlocks.Count == 0)
        {
            return HomeTimelineInteractionResult.OutsideGrid;
        }

        if (columnIndex < 0 || columnIndex >= snapshot.Columns.Count)
        {
            return HomeTimelineInteractionResult.OutsideGrid;
        }

        var clampedRatio = Clamp01(verticalPositionRatio);
        var columnDate = snapshot.Columns[columnIndex].Date;
        var hitTime = GetHitTime(snapshot, clampedRatio);
        var cellStartTime = GetCellStartTime(snapshot, clampedRatio);

        var candidates = snapshot.Items
            .Where(item => item.ColumnDate == columnDate)
            .Select(item => new TimelineItem
            {
                ItemId = item.ItemId,
                StartAt = CreateLocalTimelinePoint(columnDate, TimeOnly.FromDateTime(item.VisibleStartAt.DateTime)),
                EndAt = CreateLocalTimelinePoint(columnDate, TimeOnly.FromDateTime(item.VisibleEndAt.DateTime))
            })
            .ToArray();

        var resolvedItem = homeInteractionRuleService.ResolveHit(
            candidates,
            CreateLocalTimelinePoint(columnDate, hitTime));

        return new HomeTimelineInteractionResult
        {
            IsWithinGrid = true,
            ColumnDate = columnDate,
            CellStartTime = cellStartTime,
            HitTime = hitTime,
            HitItemId = resolvedItem?.ItemId
        };
    }

    private static TimeOnly GetHitTime(HomeLayoutSnapshot snapshot, double clampedRatio)
    {
        var planStart = snapshot.TimeBlocks[0].StartTime;
        var minutesFromStart = (int)Math.Round(snapshot.TotalVisibleMinutes * clampedRatio, MidpointRounding.AwayFromZero);
        var maxOffsetMinutes = Math.Max(0, snapshot.TotalVisibleMinutes - 1);
        return planStart.AddMinutes(Math.Min(maxOffsetMinutes, minutesFromStart));
    }

    private static TimeOnly GetCellStartTime(HomeLayoutSnapshot snapshot, double clampedRatio)
    {
        var blockIndex = (int)Math.Floor(snapshot.TimeBlocks.Count * clampedRatio);
        if (blockIndex >= snapshot.TimeBlocks.Count)
        {
            blockIndex = snapshot.TimeBlocks.Count - 1;
        }

        return snapshot.TimeBlocks[blockIndex].StartTime;
    }

    private static DateTimeOffset CreateLocalTimelinePoint(DateOnly date, TimeOnly time)
    {
        return new DateTimeOffset(date.ToDateTime(time), TimeSpan.Zero);
    }

    private static double Clamp01(double value)
    {
        if (double.IsNaN(value))
        {
            return 0d;
        }

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
}
