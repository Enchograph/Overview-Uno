using Overview.Client.Application.Ai;

namespace Overview.Client.Infrastructure.Api.Ai;

public interface IAiRemoteClient
{
    Task<string> CompleteChatAsync(
        string baseUrl,
        string apiKey,
        AiChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}
