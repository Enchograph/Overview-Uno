using Overview.Client.Domain.Entities;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Domain.Rules;

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
