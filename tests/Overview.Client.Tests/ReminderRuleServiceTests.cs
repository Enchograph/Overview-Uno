using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Tests;

public sealed class ReminderRuleServiceTests
{
    private readonly ReminderRuleService service = new();

    [Fact]
    public void NormalizeReminderConfig_SortsAndDeduplicatesTriggers()
    {
        var config = new ReminderConfig
        {
            IsEnabled = true,
            Triggers =
            [
                new ReminderTrigger { Channel = ReminderChannel.Notification, MinutesBeforeStart = 30 },
                new ReminderTrigger { Channel = ReminderChannel.Notification, MinutesBeforeStart = 10 },
                new ReminderTrigger { Channel = ReminderChannel.Notification, MinutesBeforeStart = 30 }
            ]
        };

        var normalized = service.NormalizeReminderConfig(config);

        Assert.True(normalized.IsEnabled);
        Assert.Collection(
            normalized.Triggers,
            trigger => Assert.Equal(10, trigger.MinutesBeforeStart),
            trigger => Assert.Equal(30, trigger.MinutesBeforeStart));
    }

    [Fact]
    public void ExpandOccurrences_WeeklyRule_ExpandsAcrossConfiguredDays()
    {
        var item = CreateTask(
            title: "Weekly planning",
            startAt: new DateTimeOffset(2026, 3, 9, 9, 0, 0, TimeSpan.Zero),
            endAt: new DateTimeOffset(2026, 3, 9, 10, 0, 0, TimeSpan.Zero),
            repeatRule: new RepeatRule
            {
                Frequency = RepeatFrequency.Weekly,
                Interval = 1,
                DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday],
                Count = 4
            });

        var occurrences = service.ExpandOccurrences(
            item,
            new DateTimeOffset(2026, 3, 9, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 3, 20, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(
            [
                new DateTimeOffset(2026, 3, 9, 9, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 3, 11, 9, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 3, 16, 9, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 3, 18, 9, 0, 0, TimeSpan.Zero)
            ],
            occurrences.Select(occurrence => occurrence.StartAt).ToArray());
    }

    [Fact]
    public void BuildReminderSchedule_ReturnsSortedReminderTriggersWithinRange()
    {
        var item = CreateTask(
            title: "Reminder target",
            startAt: new DateTimeOffset(2026, 3, 13, 9, 0, 0, TimeSpan.Zero),
            endAt: new DateTimeOffset(2026, 3, 13, 10, 0, 0, TimeSpan.Zero),
            reminderConfig: new ReminderConfig
            {
                IsEnabled = true,
                Triggers =
                [
                    new ReminderTrigger { Channel = ReminderChannel.Notification, MinutesBeforeStart = 30 },
                    new ReminderTrigger { Channel = ReminderChannel.Notification, MinutesBeforeStart = 5 }
                ]
            });

        var reminders = service.BuildReminderSchedule(
            item,
            new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 3, 14, 0, 0, 0, TimeSpan.Zero));

        Assert.Collection(
            reminders,
            reminder =>
            {
                Assert.Equal(new DateTimeOffset(2026, 3, 13, 8, 30, 0, TimeSpan.Zero), reminder.TriggerAt);
                Assert.Equal(30, reminder.MinutesBeforeStart);
            },
            reminder =>
            {
                Assert.Equal(new DateTimeOffset(2026, 3, 13, 8, 55, 0, TimeSpan.Zero), reminder.TriggerAt);
                Assert.Equal(5, reminder.MinutesBeforeStart);
            });
    }

    private static Item CreateTask(
        string title,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        RepeatRule? repeatRule = null,
        ReminderConfig? reminderConfig = null)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = ItemType.Task,
            Title = title,
            PlannedStartAt = startAt,
            PlannedEndAt = endAt,
            DeadlineAt = endAt.AddHours(8),
            ReminderConfig = reminderConfig ?? new ReminderConfig(),
            RepeatRule = repeatRule ?? new RepeatRule(),
            TimeZoneId = "UTC",
            CreatedAt = startAt.AddDays(-1),
            UpdatedAt = startAt.AddDays(-1),
            LastModifiedAt = startAt.AddDays(-1),
            SourceDeviceId = "test-device"
        };
    }
}
