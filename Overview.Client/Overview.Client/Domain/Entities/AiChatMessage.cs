using Overview.Client.Domain.Enums;

namespace Overview.Client.Domain.Entities;

public sealed class AiChatMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid UserId { get; init; }

    public DateOnly OccurredOn { get; init; }

    public AiChatRole Role { get; init; } = AiChatRole.User;

    public string Message { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public AiRequestType RequestType { get; init; } = AiRequestType.AnswerQuestion;

    public IReadOnlyList<Guid> LinkedItemIds { get; init; } = Array.Empty<Guid>();
}
