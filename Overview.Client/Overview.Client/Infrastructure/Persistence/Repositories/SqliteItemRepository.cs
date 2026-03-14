using Overview.Client.Domain.Entities;
using Overview.Client.Infrastructure.Persistence.Records;
using Overview.Client.Infrastructure.Persistence.Services;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public sealed class SqliteItemRepository : IItemRepository
{
    private readonly ISqliteConnectionFactory connectionFactory;

    public SqliteItemRepository(ISqliteConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task<Item?> GetAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        var userIdValue = userId.ToString();
        var itemIdValue = itemId.ToString();
        var record = await connection.Table<ItemRecord>()
            .Where(row => row.UserId == userIdValue && row.Id == itemIdValue)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return record is null ? null : ClientJsonSerializer.Deserialize<Item>(record.PayloadJson);
    }

    public async Task<IReadOnlyList<Item>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        var userIdValue = userId.ToString();
        var records = await connection.Table<ItemRecord>()
            .Where(row => row.UserId == userIdValue)
            .OrderByDescending(row => row.LastModifiedAtTicks)
            .ToListAsync()
            .ConfigureAwait(false);

        return records.Select(record => ClientJsonSerializer.Deserialize<Item>(record.PayloadJson)).ToArray();
    }

    public async Task UpsertAsync(Item item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        await connection.InsertOrReplaceAsync(new ItemRecord
        {
            Id = item.Id.ToString(),
            UserId = item.UserId.ToString(),
            LastModifiedAtTicks = item.LastModifiedAt.UtcTicks,
            IsDeleted = item.DeletedAt is not null,
            ItemType = item.Type.ToString(),
            Title = item.Title,
            PayloadJson = ClientJsonSerializer.Serialize(item),
        }).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        var userIdValue = userId.ToString();
        var itemIdValue = itemId.ToString();
        await connection.Table<ItemRecord>()
            .DeleteAsync(row => row.UserId == userIdValue && row.Id == itemIdValue)
            .ConfigureAwait(false);
    }
}
