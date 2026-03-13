using Overview.Client.Domain.Entities;

namespace Overview.Client.Application.Ai;

public interface IAiChatService
{
    Task<AiChatDaySnapshot> GetTodaySnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<AiChatDaySnapshot> SendMessageAsync(
        Guid userId,
        string userMessage,
        CancellationToken cancellationToken = default);
}
