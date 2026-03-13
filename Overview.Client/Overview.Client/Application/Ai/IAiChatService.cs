using Overview.Client.Domain.Entities;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Application.Ai;

public interface IAiChatService
{
    Task<AiChatPeriodSnapshot> GetSnapshotAsync(
        Guid userId,
        CalendarPeriod period,
        CancellationToken cancellationToken = default);

    Task<AiChatDaySnapshot> GetTodaySnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<AiChatDaySnapshot> SendMessageAsync(
        Guid userId,
        string userMessage,
        CancellationToken cancellationToken = default);
}
