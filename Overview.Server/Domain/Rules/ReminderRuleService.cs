using Overview.Server.Domain.Entities;
using Overview.Server.Domain.Enums;
using Overview.Server.Domain.ValueObjects;

namespace Overview.Server.Domain.Rules;

public sealed class ReminderRuleService : IReminderRuleService
{
    public ReminderConfig NormalizeReminderConfig(ReminderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!config.IsEnabled || config.Triggers.Count == 0)
        {
            return new ReminderConfig
            {
                IsEnabled = false,
                Triggers = Array.Empty<ReminderTrigger>(),
            };
        }

        var normalizedTriggers = config.Triggers
            .Select(trigger =>
            {
                if (trigger.MinutesBeforeStart < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(config), "Reminder minutes cannot be negative.");
                }

                return trigger;
            })
            .Distinct()
            .OrderBy(trigger => trigger.MinutesBeforeStart)
            .ThenBy(trigger => trigger.Channel)
            .ToArray();

        return new ReminderConfig
        {
            IsEnabled = normalizedTriggers.Length > 0,
            Triggers = normalizedTriggers,
        };
    }

    public IReadOnlyList<ItemOccurrence> ExpandOccurrences(
        Item item,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int maxOccurrences = 512)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateRange(rangeStart, rangeEnd, maxOccurrences);

        if (item.DeletedAt is not null)
        {
            return Array.Empty<ItemOccurrence>();
        }

        var seed = GetOccurrenceSeed(item);
        if (seed is null)
        {
            return Array.Empty<ItemOccurrence>();
        }

        var rule = item.RepeatRule ?? new RepeatRule();
        ValidateRepeatRule(rule);

        if (rule.Frequency == RepeatFrequency.None)
        {
            var occurrence = BuildOccurrence(seed.StartAt, seed.Duration);
            return IntersectsRange(occurrence, rangeStart, rangeEnd)
                ? new[] { occurrence }
                : Array.Empty<ItemOccurrence>();
        }

        var timeZone = ResolveTimeZone(item.TimeZoneId);
        var seedLocalStart = TimeZoneInfo.ConvertTime(seed.StartAt, timeZone);
        var untilUtc = rule.UntilAt?.ToUniversalTime();
        var occurrences = new List<ItemOccurrence>(Math.Min(maxOccurrences, 64));

        switch (rule.Frequency)
        {
            case RepeatFrequency.Daily:
                ExpandDailyOccurrences(seed, seedLocalStart, rule, rangeStart, rangeEnd, untilUtc, maxOccurrences, occurrences);
                break;
            case RepeatFrequency.Weekly:
                ExpandWeeklyOccurrences(seed, seedLocalStart, rule, rangeStart, rangeEnd, untilUtc, maxOccurrences, timeZone, occurrences);
                break;
            case RepeatFrequency.Monthly:
                ExpandMonthlyOccurrences(seed, seedLocalStart, rule, rangeStart, rangeEnd, untilUtc, maxOccurrences, timeZone, occurrences);
                break;
            case RepeatFrequency.Yearly:
                ExpandYearlyOccurrences(seed, seedLocalStart, rule, rangeStart, rangeEnd, untilUtc, maxOccurrences, timeZone, occurrences);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(item), rule.Frequency, "Unsupported repeat frequency.");
        }

        return occurrences;
    }

    public IReadOnlyList<ScheduledReminder> BuildReminderSchedule(
        Item item,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int maxOccurrences = 512)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateRange(rangeStart, rangeEnd, maxOccurrences);

        var config = NormalizeReminderConfig(item.ReminderConfig);
        if (!config.IsEnabled)
        {
            return Array.Empty<ScheduledReminder>();
        }

        var reminders = new List<ScheduledReminder>();

        foreach (var occurrence in ExpandOccurrences(item, rangeStart.AddDays(-365), rangeEnd, maxOccurrences))
        {
            foreach (var trigger in config.Triggers)
            {
                var triggerAt = occurrence.StartAt.AddMinutes(-trigger.MinutesBeforeStart);
                if (triggerAt < rangeStart || triggerAt >= rangeEnd)
                {
                    continue;
                }

                reminders.Add(new ScheduledReminder
                {
                    Channel = trigger.Channel,
                    MinutesBeforeStart = trigger.MinutesBeforeStart,
                    TriggerAt = triggerAt,
                    OccurrenceStartAt = occurrence.StartAt,
                    OccurrenceEndAt = occurrence.EndAt,
                });
            }
        }

        return reminders
            .OrderBy(reminder => reminder.TriggerAt)
            .ThenBy(reminder => reminder.Channel)
            .ToArray();
    }

    private static void ExpandDailyOccurrences(
        OccurrenceSeed seed,
        DateTimeOffset seedLocalStart,
        RepeatRule rule,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        DateTimeOffset? untilUtc,
        int maxOccurrences,
        List<ItemOccurrence> occurrences)
    {
        for (var index = 0; ShouldContinue(index, maxOccurrences, rule.Count); index++)
        {
            var occurrence = BuildOccurrence(seed.StartAt.AddDays((long)index * rule.Interval), seed.Duration);
            if (ShouldStop(occurrence.StartAt, rangeEnd, untilUtc))
            {
                break;
            }

            AddIfInRange(occurrence, rangeStart, rangeEnd, occurrences);
        }
    }

    private static void ExpandWeeklyOccurrences(
        OccurrenceSeed seed,
        DateTimeOffset seedLocalStart,
        RepeatRule rule,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        DateTimeOffset? untilUtc,
        int maxOccurrences,
        TimeZoneInfo timeZone,
        List<ItemOccurrence> occurrences)
    {
        var allowedDays = (rule.DaysOfWeek.Count == 0 ? new[] { seedLocalStart.DayOfWeek } : rule.DaysOfWeek)
            .Distinct()
            .OrderBy(day => day)
            .ToArray();
        var weekStart = seedLocalStart.Date.AddDays(-GetDayOffset(seedLocalStart.DayOfWeek, DayOfWeek.Monday));
        var produced = 0;

        for (var weekIndex = 0; produced < maxOccurrences; weekIndex += rule.Interval)
        {
            var weekBase = weekStart.AddDays(weekIndex * 7L);

            foreach (var day in allowedDays)
            {
                if (produced >= maxOccurrences || (rule.Count is not null && produced >= rule.Count.Value))
                {
                    return;
                }

                var occurrenceDate = DateOnly.FromDateTime(weekBase.AddDays(GetDayOffset(day, DayOfWeek.Monday)));
                var occurrenceStart = ConvertLocalToUtc(occurrenceDate, TimeOnly.FromDateTime(seedLocalStart.DateTime), timeZone);
                if (occurrenceStart < seed.StartAt)
                {
                    continue;
                }

                if (ShouldStop(occurrenceStart, rangeEnd, untilUtc))
                {
                    return;
                }

                var occurrence = BuildOccurrence(occurrenceStart, seed.Duration);
                produced++;
                AddIfInRange(occurrence, rangeStart, rangeEnd, occurrences);
            }
        }
    }

    private static void ExpandMonthlyOccurrences(
        OccurrenceSeed seed,
        DateTimeOffset seedLocalStart,
        RepeatRule rule,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        DateTimeOffset? untilUtc,
        int maxOccurrences,
        TimeZoneInfo timeZone,
        List<ItemOccurrence> occurrences)
    {
        var dayOfMonth = rule.DayOfMonth ?? seedLocalStart.Day;
        var localTime = TimeOnly.FromDateTime(seedLocalStart.DateTime);

        for (var index = 0; ShouldContinue(index, maxOccurrences, rule.Count); index++)
        {
            var monthBase = new DateOnly(seedLocalStart.Year, seedLocalStart.Month, 1).AddMonths(index * rule.Interval);
            var day = Math.Min(dayOfMonth, DateTime.DaysInMonth(monthBase.Year, monthBase.Month));
            var occurrenceStart = ConvertLocalToUtc(new DateOnly(monthBase.Year, monthBase.Month, day), localTime, timeZone);
            if (occurrenceStart < seed.StartAt)
            {
                continue;
            }

            if (ShouldStop(occurrenceStart, rangeEnd, untilUtc))
            {
                break;
            }

            AddIfInRange(BuildOccurrence(occurrenceStart, seed.Duration), rangeStart, rangeEnd, occurrences);
        }
    }

    private static void ExpandYearlyOccurrences(
        OccurrenceSeed seed,
        DateTimeOffset seedLocalStart,
        RepeatRule rule,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        DateTimeOffset? untilUtc,
        int maxOccurrences,
        TimeZoneInfo timeZone,
        List<ItemOccurrence> occurrences)
    {
        var month = rule.MonthOfYear ?? seedLocalStart.Month;
        var dayOfMonth = rule.DayOfMonth ?? seedLocalStart.Day;
        var localTime = TimeOnly.FromDateTime(seedLocalStart.DateTime);

        for (var index = 0; ShouldContinue(index, maxOccurrences, rule.Count); index++)
        {
            var year = seedLocalStart.Year + (index * rule.Interval);
            var day = Math.Min(dayOfMonth, DateTime.DaysInMonth(year, month));
            var occurrenceStart = ConvertLocalToUtc(new DateOnly(year, month, day), localTime, timeZone);
            if (occurrenceStart < seed.StartAt)
            {
                continue;
            }

            if (ShouldStop(occurrenceStart, rangeEnd, untilUtc))
            {
                break;
            }

            AddIfInRange(BuildOccurrence(occurrenceStart, seed.Duration), rangeStart, rangeEnd, occurrences);
        }
    }

    private static OccurrenceSeed? GetOccurrenceSeed(Item item)
    {
        return item.Type switch
        {
            ItemType.Schedule when item.StartAt is not null => new OccurrenceSeed(
                item.StartAt.Value.ToUniversalTime(),
                item.EndAt?.ToUniversalTime() - item.StartAt?.ToUniversalTime()),
            ItemType.Task when item.PlannedStartAt is not null => new OccurrenceSeed(
                item.PlannedStartAt.Value.ToUniversalTime(),
                item.PlannedEndAt?.ToUniversalTime() - item.PlannedStartAt?.ToUniversalTime()),
            ItemType.Note when item.TargetDate is not null => BuildNoteSeed(item),
            _ => null,
        };
    }

    private static OccurrenceSeed BuildNoteSeed(Item item)
    {
        var timeZone = ResolveTimeZone(item.TimeZoneId);
        var duration = item.ExpectedDurationMinutes is > 0
            ? TimeSpan.FromMinutes(item.ExpectedDurationMinutes.Value)
            : (TimeSpan?)null;
        var startAt = ConvertLocalToUtc(item.TargetDate!.Value, TimeOnly.MinValue, timeZone);
        return new OccurrenceSeed(startAt, duration);
    }

    private static ItemOccurrence BuildOccurrence(DateTimeOffset startAt, TimeSpan? duration)
    {
        return new ItemOccurrence
        {
            StartAt = startAt,
            EndAt = duration is null ? null : startAt.Add(duration.Value),
        };
    }

    private static bool IntersectsRange(ItemOccurrence occurrence, DateTimeOffset rangeStart, DateTimeOffset rangeEnd)
    {
        var endAt = occurrence.EndAt ?? occurrence.StartAt;
        return endAt >= rangeStart && occurrence.StartAt < rangeEnd;
    }

    private static void AddIfInRange(
        ItemOccurrence occurrence,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        List<ItemOccurrence> occurrences)
    {
        if (IntersectsRange(occurrence, rangeStart, rangeEnd))
        {
            occurrences.Add(occurrence);
        }
    }

    private static void ValidateRange(DateTimeOffset rangeStart, DateTimeOffset rangeEnd, int maxOccurrences)
    {
        if (rangeEnd <= rangeStart)
        {
            throw new ArgumentOutOfRangeException(nameof(rangeEnd), "Range end must be greater than range start.");
        }

        if (maxOccurrences <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxOccurrences), "Max occurrences must be greater than zero.");
        }
    }

    private static void ValidateRepeatRule(RepeatRule rule)
    {
        if (rule.Interval <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rule), "Repeat interval must be greater than zero.");
        }

        if (rule.Count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rule), "Repeat count must be greater than zero when provided.");
        }

        if (rule.DayOfMonth is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(rule), "Day of month must be between 1 and 31.");
        }

        if (rule.MonthOfYear is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(rule), "Month of year must be between 1 and 12.");
        }
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static DateTimeOffset ConvertLocalToUtc(DateOnly date, TimeOnly time, TimeZoneInfo timeZone)
    {
        var localDateTime = date.ToDateTime(time, DateTimeKind.Unspecified);
        return new DateTimeOffset(localDateTime, timeZone.GetUtcOffset(localDateTime)).ToUniversalTime();
    }

    private static int GetDayOffset(DayOfWeek day, DayOfWeek weekStartDay)
    {
        return (7 + (int)day - (int)weekStartDay) % 7;
    }

    private static bool ShouldContinue(int index, int maxOccurrences, int? count)
    {
        return index < maxOccurrences && (count is null || index < count.Value);
    }

    private static bool ShouldStop(DateTimeOffset startAt, DateTimeOffset rangeEnd, DateTimeOffset? untilUtc)
    {
        return startAt >= rangeEnd || (untilUtc is not null && startAt > untilUtc.Value);
    }

    private sealed record OccurrenceSeed(DateTimeOffset StartAt, TimeSpan? Duration);
}
