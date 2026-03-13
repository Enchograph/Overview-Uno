using Overview.Client.Domain.Enums;

namespace Overview.Client.Domain.ValueObjects;

public sealed record CalendarPeriod
{
    public required TimeSelectionMode Mode { get; init; }

    public required DateOnly ReferenceDate { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }
}
