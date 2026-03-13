using Overview.Server.Domain.Entities;
using Overview.Server.Domain.ValueObjects;

namespace Overview.Server.Domain.Rules;

public interface IReminderRuleService
{
    ReminderConfig NormalizeReminderConfig(ReminderConfig config);

    IReadOnlyList<ItemOccurrence> ExpandOccurrences(
        Item item,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int maxOccurrences = 512);

    IReadOnlyList<ScheduledReminder> BuildReminderSchedule(
        Item item,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int maxOccurrences = 512);
}
