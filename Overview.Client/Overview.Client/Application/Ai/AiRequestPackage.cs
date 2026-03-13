using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Ai;

public sealed record AiRequestPackage
{
    public string BaseUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public bool HasRequiredConfiguration { get; init; }

    public AiRequestType RequestType { get; init; } = AiRequestType.AnswerQuestion;

    public string SystemPrompt { get; init; } = string.Empty;

    public string UserMessage { get; init; } = string.Empty;

    public IReadOnlyList<AiItemSummary> RelevantItems { get; init; } = Array.Empty<AiItemSummary>();

    public AiChatCompletionRequest RequestBody { get; init; } = new();
}
