namespace Overview.Client.Application.Ai;

public sealed record AiChatCompletionRequest
{
    public string Model { get; init; } = string.Empty;

    public string ResponseFormat { get; init; } = "json_object";

    public IReadOnlyList<AiChatCompletionMessage> Messages { get; init; } = Array.Empty<AiChatCompletionMessage>();
}
