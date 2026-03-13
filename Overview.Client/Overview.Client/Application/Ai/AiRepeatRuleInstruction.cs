using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Ai;

public sealed record AiRepeatRuleInstruction
{
    public RepeatFrequency Frequency { get; init; } = RepeatFrequency.None;

    public int Interval { get; init; } = 1;

    public IReadOnlyList<DayOfWeek> DaysOfWeek { get; init; } = Array.Empty<DayOfWeek>();

    public int? DayOfMonth { get; init; }

    public int? MonthOfYear { get; init; }

    public DateTimeOffset? UntilAt { get; init; }

    public int? Count { get; init; }
}
