using Overview.Server.Domain.Enums;

namespace Overview.Server.Domain.ValueObjects;

public sealed record CalendarPeriod
{
    public required TimeSelectionMode Mode { get; init; }

    public required DateOnly ReferenceDate { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }
}
