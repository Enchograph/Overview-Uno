using Overview.Client.Domain.Entities;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public interface ISyncChangeRepository
{
    Task<IReadOnlyList<SyncChange>> ListPendingAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpsertAsync(SyncChange change, CancellationToken cancellationToken = default);

    Task MarkSyncedAsync(
        IEnumerable<Guid> changeIds,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(IEnumerable<Guid> changeIds, CancellationToken cancellationToken = default);
}
