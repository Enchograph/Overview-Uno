using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Domain.Rules;

public sealed class HomeInteractionRuleService : IHomeInteractionRuleService
{
    public IReadOnlyList<TimelineItemOverlap> CalculateOverlapStates(IReadOnlyList<TimelineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var normalizedItems = Normalize(items);
        if (normalizedItems.Count == 0)
        {
            return Array.Empty<TimelineItemOverlap>();
        }

        var overlaps = new TimelineItemOverlap[normalizedItems.Count];

        for (var index = 0; index < normalizedItems.Count; index++)
        {
            var maxConcurrentCount = GetMaxConcurrentCount(normalizedItems[index], normalizedItems);

            overlaps[index] = new TimelineItemOverlap
            {
                ItemId = normalizedItems[index].ItemId,
                MaxConcurrentCount = maxConcurrentCount,
                Opacity = 1d / maxConcurrentCount,
            };
        }

        return overlaps;
    }

    public TimelineItem? ResolveHit(IReadOnlyList<TimelineItem> items, DateTimeOffset hitTime)
    {
        ArgumentNullException.ThrowIfNull(items);

        var candidates = Normalize(items)
            .Where(item => item.StartAt <= hitTime && hitTime < item.EndAt)
            .ToArray();

        if (candidates.Length == 0)
        {
            return null;
        }

        if (candidates.Length == 1)
        {
            return candidates[0];
        }

        var wrappedCandidates = candidates
            .Where(candidate => !HasExclusiveSegment(candidate, candidates))
            .OrderBy(candidate => candidate.StartAt)
            .ThenBy(candidate => candidate.EndAt)
            .ThenBy(candidate => candidate.ItemId)
            .ToArray();

        if (wrappedCandidates.Length == 1)
        {
            return wrappedCandidates[0];
        }

        if (wrappedCandidates.Length > 1)
        {
            return wrappedCandidates[0];
        }

        return candidates
            .OrderBy(candidate => candidate.StartAt)
            .ThenBy(candidate => candidate.EndAt)
            .ThenBy(candidate => candidate.ItemId)
            .First();
    }

    private static List<TimelineItem> Normalize(IReadOnlyList<TimelineItem> items)
    {
        var normalizedItems = new List<TimelineItem>(items.Count);

        foreach (var item in items)
        {
            if (item.EndAt <= item.StartAt)
            {
                throw new ArgumentOutOfRangeException(nameof(items), "Timeline item end must be greater than start.");
            }

            normalizedItems.Add(item);
        }

        return normalizedItems;
    }

    private static int GetMaxConcurrentCount(TimelineItem target, IReadOnlyList<TimelineItem> items)
    {
        var boundaries = new SortedSet<long>
        {
            target.StartAt.UtcTicks,
            target.EndAt.UtcTicks,
        };

        foreach (var item in items)
        {
            if (!Intersects(target, item))
            {
                continue;
            }

            boundaries.Add(Max(target.StartAt.UtcTicks, item.StartAt.UtcTicks));
            boundaries.Add(Min(target.EndAt.UtcTicks, item.EndAt.UtcTicks));
        }

        var orderedBoundaries = boundaries.ToArray();
        var maxConcurrentCount = 1;

        for (var index = 0; index < orderedBoundaries.Length - 1; index++)
        {
            var segmentStart = orderedBoundaries[index];
            var segmentEnd = orderedBoundaries[index + 1];
            if (segmentEnd <= segmentStart)
            {
                continue;
            }

            var sampleTick = segmentStart + ((segmentEnd - segmentStart) / 2);
            var sampleTime = new DateTimeOffset(sampleTick, TimeSpan.Zero);
            var concurrentCount = items.Count(item => item.StartAt <= sampleTime && sampleTime < item.EndAt);
            maxConcurrentCount = Math.Max(maxConcurrentCount, concurrentCount);
        }

        return maxConcurrentCount;
    }

    private static bool HasExclusiveSegment(TimelineItem target, IReadOnlyList<TimelineItem> candidates)
    {
        var boundaries = new SortedSet<long>
        {
            target.StartAt.UtcTicks,
            target.EndAt.UtcTicks,
        };

        foreach (var candidate in candidates)
        {
            if (candidate.ItemId == target.ItemId || !Intersects(target, candidate))
            {
                continue;
            }

            boundaries.Add(Max(target.StartAt.UtcTicks, candidate.StartAt.UtcTicks));
            boundaries.Add(Min(target.EndAt.UtcTicks, candidate.EndAt.UtcTicks));
        }

        var orderedBoundaries = boundaries.ToArray();

        for (var index = 0; index < orderedBoundaries.Length - 1; index++)
        {
            var segmentStart = orderedBoundaries[index];
            var segmentEnd = orderedBoundaries[index + 1];
            if (segmentEnd <= segmentStart)
            {
                continue;
            }

            var sampleTick = segmentStart + ((segmentEnd - segmentStart) / 2);
            var sampleTime = new DateTimeOffset(sampleTick, TimeSpan.Zero);
            var activeCandidates = candidates.Count(candidate => candidate.StartAt <= sampleTime && sampleTime < candidate.EndAt);

            if (activeCandidates == 1 && target.StartAt <= sampleTime && sampleTime < target.EndAt)
            {
                return true;
            }
        }

        return false;
    }

    private static bool Intersects(TimelineItem left, TimelineItem right)
    {
        return left.StartAt < right.EndAt && right.StartAt < left.EndAt;
    }

    private static long Max(long left, long right)
    {
        return left > right ? left : right;
    }

    private static long Min(long left, long right)
    {
        return left < right ? left : right;
    }
}
