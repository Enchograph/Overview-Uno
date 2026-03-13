using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Domain.Rules;

public interface IHomeInteractionRuleService
{
    IReadOnlyList<TimelineItemOverlap> CalculateOverlapStates(IReadOnlyList<TimelineItem> items);

    TimelineItem? ResolveHit(IReadOnlyList<TimelineItem> items, DateTimeOffset hitTime);
}
