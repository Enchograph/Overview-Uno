using Overview.Client.Application.Home;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Tests;

public sealed class HomeTimelineInteractionServiceTests
{
    private static readonly DateOnly TestDate = new(2026, 3, 13);

    private readonly HomeTimelineInteractionService service = new(new HomeInteractionRuleService());

    [Fact]
    public void ResolveInteraction_ReturnsEarliestExclusiveItem_WhenAllCandidatesHaveExclusiveSegments()
    {
        var snapshot = CreateSnapshot(
            CreateItem("Early", 9, 0, 10, 0),
            CreateItem("Late", 9, 30, 10, 30));

        var result = service.ResolveInteraction(snapshot, columnIndex: 0, verticalPositionRatio: 0.25d);

        Assert.True(result.IsWithinGrid);
        Assert.Equal(TestDate, result.ColumnDate);
        Assert.Equal(new TimeOnly(9, 0), result.HitTime);
        Assert.Equal(new TimeOnly(9, 0), result.CellStartTime);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), result.HitItemId);
    }

    [Fact]
    public void ResolveInteraction_ReturnsWrappedItem_WhenExactlyOneCandidateIsFullyWrapped()
    {
        var snapshot = CreateSnapshot(
            CreateItem("Outer", 9, 0, 11, 0, "11111111-1111-1111-1111-111111111111"),
            CreateItem("Inner", 9, 30, 10, 0, "22222222-2222-2222-2222-222222222222"));

        var result = service.ResolveInteraction(snapshot, columnIndex: 0, verticalPositionRatio: 0.4375d);

        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), result.HitItemId);
        Assert.Equal(new TimeOnly(9, 45), result.HitTime);
        Assert.Equal(new TimeOnly(9, 0), result.CellStartTime);
    }

    [Fact]
    public void ResolveInteraction_ReturnsCellStartTime_WhenNoItemIsHit()
    {
        var snapshot = CreateSnapshot(CreateItem("Only", 9, 0, 10, 0));

        var result = service.ResolveInteraction(snapshot, columnIndex: 0, verticalPositionRatio: 0.75d);

        Assert.True(result.IsWithinGrid);
        Assert.Null(result.HitItemId);
        Assert.Equal(new TimeOnly(11, 0), result.HitTime);
        Assert.Equal(new TimeOnly(11, 0), result.CellStartTime);
    }

    [Fact]
    public void ResolveInteraction_ReturnsOutsideGrid_WhenColumnIndexIsInvalid()
    {
        var snapshot = CreateSnapshot(CreateItem("Only", 9, 0, 10, 0));

        var result = service.ResolveInteraction(snapshot, columnIndex: 3, verticalPositionRatio: 0.1d);

        Assert.False(result.IsWithinGrid);
        Assert.Null(result.HitItemId);
    }

    private static HomeLayoutSnapshot CreateSnapshot(params HomeLayoutItem[] items)
    {
        return new HomeLayoutSnapshot
        {
            ViewMode = HomeViewMode.Week,
            Period = new CalendarPeriod
            {
                Mode = TimeSelectionMode.Week,
                StartDate = TestDate,
                EndDate = TestDate,
                ReferenceDate = TestDate
            },
            Title = "Test",
            Columns =
            [
                new HomeDateColumn
                {
                    Date = TestDate,
                    HeaderLabel = "Fri 3/13",
                    IsToday = false
                }
            ],
            TimeBlocks =
            [
                CreateTimeBlock(0, 8, 0, 9, 0),
                CreateTimeBlock(1, 9, 0, 10, 0),
                CreateTimeBlock(2, 10, 0, 11, 0),
                CreateTimeBlock(3, 11, 0, 12, 0)
            ],
            Items = items,
            TotalVisibleMinutes = 240
        };
    }

    private static HomeLayoutItem CreateItem(
        string title,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute,
        string itemId = "11111111-1111-1111-1111-111111111111")
    {
        var visibleStart = new DateTimeOffset(TestDate.ToDateTime(new TimeOnly(startHour, startMinute)), TimeSpan.Zero);
        var visibleEnd = new DateTimeOffset(TestDate.ToDateTime(new TimeOnly(endHour, endMinute)), TimeSpan.Zero);

        return new HomeLayoutItem
        {
            ItemId = Guid.Parse(itemId),
            ColumnDate = TestDate,
            Type = ItemType.Task,
            Title = title,
            VisibleStartAt = visibleStart,
            VisibleEndAt = visibleEnd,
            TopRatio = 0d,
            HeightRatio = 0.25d,
            Opacity = 1d,
            IsClippedAtStart = false,
            IsClippedAtEnd = false
        };
    }

    private static TimeBlockDefinition CreateTimeBlock(
        int index,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        return new TimeBlockDefinition
        {
            Index = index,
            StartTime = new TimeOnly(startHour, startMinute),
            EndTime = new TimeOnly(endHour, endMinute),
            DurationMinutes = 60,
            GapMinutes = 0
        };
    }
}
