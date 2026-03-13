using Overview.Client.Domain.Entities;
using Overview.Client.Infrastructure.Persistence.Records;
using Overview.Client.Infrastructure.Persistence.Services;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public sealed class SqliteSyncChangeRepository : ISyncChangeRepository
{
    private readonly ISqliteConnectionFactory connectionFactory;

    public SqliteSyncChangeRepository(ISqliteConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<SyncChange>> ListPendingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        var records = await connection.Table<SyncChangeRecord>()
            .Where(row => row.UserId == userId.ToString() && row.SyncedAtTicks == null)
            .OrderBy(row => row.LastModifiedAtTicks)
            .ToListAsync()
            .ConfigureAwait(false);

        return records.Select(record => ClientJsonSerializer.Deserialize<SyncChange>(record.PayloadJson)).ToArray();
    }

    public async Task UpsertAsync(SyncChange change, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(change);
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        await connection.InsertOrReplaceAsync(new SyncChangeRecord
        {
            Id = change.Id.ToString(),
            UserId = change.UserId.ToString(),
            LastModifiedAtTicks = change.LastModifiedAt.UtcTicks,
            SyncedAtTicks = change.SyncedAt?.UtcTicks,
            EntityType = change.EntityType.ToString(),
            PayloadJson = ClientJsonSerializer.Serialize(change),
        }).ConfigureAwait(false);
    }
}
