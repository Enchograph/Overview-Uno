using System.Globalization;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;

namespace Overview.Client.Tests;

public sealed class TimeRuleServiceTests
{
    private readonly TimeRuleService service = new();

    [Fact]
    public void BuildTimeBlocks_UsesConfiguredStartDurationGapAndCount()
    {
        var settings = new UserSettings
        {
            UserId = Guid.NewGuid(),
            DayPlanStartTime = new TimeOnly(8, 30),
            TimeBlockDurationMinutes = 90,
            TimeBlockGapMinutes = 15,
            TimeBlockCount = 3
        };

        var blocks = service.BuildTimeBlocks(settings);

        Assert.Collection(
            blocks,
            block =>
            {
                Assert.Equal(0, block.Index);
                Assert.Equal(new TimeOnly(8, 30), block.StartTime);
                Assert.Equal(new TimeOnly(10, 0), block.EndTime);
                Assert.Equal("08:30~10:00", block.Label);
            },
            block =>
            {
                Assert.Equal(1, block.Index);
                Assert.Equal(new TimeOnly(10, 15), block.StartTime);
                Assert.Equal(new TimeOnly(11, 45), block.EndTime);
            },
            block =>
            {
                Assert.Equal(2, block.Index);
                Assert.Equal(new TimeOnly(12, 0), block.StartTime);
                Assert.Equal(new TimeOnly(13, 30), block.EndTime);
            });
    }

    [Fact]
    public void GetPeriod_WeekMode_RespectsConfiguredWeekStartDay()
    {
        var period = service.GetPeriod(
            new DateOnly(2026, 3, 13),
            TimeSelectionMode.Week,
            DayOfWeek.Sunday);

        Assert.Equal(TimeSelectionMode.Week, period.Mode);
        Assert.Equal(new DateOnly(2026, 3, 8), period.StartDate);
        Assert.Equal(new DateOnly(2026, 3, 14), period.EndDate);
    }

    [Fact]
    public void FormatPeriodTitle_ReturnsLocalizedTitlesForChineseAndEnglish()
    {
        var weekPeriod = service.GetPeriod(
            new DateOnly(2026, 3, 13),
            TimeSelectionMode.Week,
            DayOfWeek.Monday);
        var dayPeriod = service.GetPeriod(
            new DateOnly(2026, 3, 13),
            TimeSelectionMode.Day,
            DayOfWeek.Monday);

        var zhWeekTitle = service.FormatPeriodTitle(weekPeriod, DayOfWeek.Monday, CultureInfo.GetCultureInfo("zh-CN"));
        var enWeekTitle = service.FormatPeriodTitle(weekPeriod, DayOfWeek.Monday, CultureInfo.GetCultureInfo("en-US"));
        var zhDayTitle = service.FormatPeriodTitle(dayPeriod, DayOfWeek.Monday, CultureInfo.GetCultureInfo("zh-CN"));

        Assert.Equal("2026年3月 第3周", zhWeekTitle);
        Assert.Equal("March 2026 - Week 3", enWeekTitle);
        Assert.Equal("2026/3/13", zhDayTitle);
    }
}
