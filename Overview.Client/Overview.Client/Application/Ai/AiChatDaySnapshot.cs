using Overview.Client.Domain.Entities;

namespace Overview.Client.Application.Ai;

public sealed record AiChatDaySnapshot
{
    public DateOnly OccurredOn { get; init; }

    public string TimeZoneId { get; init; } = "UTC";

    public IReadOnlyList<AiChatMessage> Messages { get; init; } = Array.Empty<AiChatMessage>();
}
