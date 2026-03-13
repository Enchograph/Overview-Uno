using Overview.Client.Domain.Entities;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Ai;

public sealed record AiChatPeriodSnapshot
{
    public required CalendarPeriod Period { get; init; }

    public string TimeZoneId { get; init; } = "UTC";

    public IReadOnlyList<AiChatMessage> Messages { get; init; } = Array.Empty<AiChatMessage>();
}
