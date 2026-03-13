using Overview.Client.Domain.Entities;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public interface IAiChatMessageRepository
{
    Task<IReadOnlyList<AiChatMessage>> ListByDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(AiChatMessage message, CancellationToken cancellationToken = default);
}
