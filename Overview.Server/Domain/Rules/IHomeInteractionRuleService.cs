using Overview.Server.Domain.ValueObjects;

namespace Overview.Server.Domain.Rules;

public interface IHomeInteractionRuleService
{
    IReadOnlyList<TimelineItemOverlap> CalculateOverlapStates(IReadOnlyList<TimelineItem> items);

    TimelineItem? ResolveHit(IReadOnlyList<TimelineItem> items, DateTimeOffset hitTime);
}
