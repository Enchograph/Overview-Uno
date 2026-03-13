namespace Overview.Client.Application.Ai;

public interface IAiOrchestrationService
{
    Task<AiRequestPackage> BuildRequestAsync(
        Guid userId,
        string userMessage,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiItemSummary>> SearchRelevantItemsAsync(
        Guid userId,
        string userMessage,
        int maxCount = 8,
        CancellationToken cancellationToken = default);

    AiParseResult ParseResponse(string responseContent);
}
