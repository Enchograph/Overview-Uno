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

    public async Task MarkSyncedAsync(
        IEnumerable<Guid> changeIds,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changeIds);
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        foreach (var changeId in changeIds.Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var record = await connection.FindAsync<SyncChangeRecord>(changeId.ToString()).ConfigureAwait(false);
            if (record is null)
            {
                continue;
            }

            var change = ClientJsonSerializer.Deserialize<SyncChange>(record.PayloadJson);
            var syncedChange = new SyncChange
            {
                Id = change.Id,
                UserId = change.UserId,
                DeviceId = change.DeviceId,
                EntityType = change.EntityType,
                ChangeType = change.ChangeType,
                EntityId = change.EntityId,
                ItemSnapshot = change.ItemSnapshot,
                SettingsSnapshot = change.SettingsSnapshot,
                CreatedAt = change.CreatedAt,
                LastModifiedAt = change.LastModifiedAt,
                SyncedAt = syncedAt
            };

            record.LastModifiedAtTicks = syncedChange.LastModifiedAt.UtcTicks;
            record.SyncedAtTicks = syncedAt.UtcTicks;
            record.PayloadJson = ClientJsonSerializer.Serialize(syncedChange);
            await connection.InsertOrReplaceAsync(record).ConfigureAwait(false);
        }
    }

    public async Task DeleteAsync(IEnumerable<Guid> changeIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changeIds);
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        foreach (var changeId in changeIds.Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await connection.DeleteAsync<SyncChangeRecord>(changeId.ToString()).ConfigureAwait(false);
        }
    }
}
