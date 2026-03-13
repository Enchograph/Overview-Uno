using Overview.Client.Domain.Entities;
using Overview.Client.Infrastructure.Persistence.Records;
using Overview.Client.Infrastructure.Persistence.Services;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public sealed class SqliteAiChatMessageRepository : IAiChatMessageRepository
{
    private readonly ISqliteConnectionFactory connectionFactory;

    public SqliteAiChatMessageRepository(ISqliteConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AiChatMessage>> ListByDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        var records = await connection.Table<AiChatMessageRecord>()
            .Where(row => row.UserId == userId.ToString() && string.CompareOrdinal(row.OccurredOn, startDate.ToString("O")) >= 0 && string.CompareOrdinal(row.OccurredOn, endDate.ToString("O")) <= 0)
            .OrderBy(row => row.CreatedAtTicks)
            .ToListAsync()
            .ConfigureAwait(false);

        return records.Select(record => ClientJsonSerializer.Deserialize<AiChatMessage>(record.PayloadJson)).ToArray();
    }

    public async Task UpsertAsync(AiChatMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        await connection.InsertOrReplaceAsync(new AiChatMessageRecord
        {
            Id = message.Id.ToString(),
            UserId = message.UserId.ToString(),
            OccurredOn = message.OccurredOn.ToString("O"),
            CreatedAtTicks = message.CreatedAt.UtcTicks,
            PayloadJson = ClientJsonSerializer.Serialize(message),
        }).ConfigureAwait(false);
    }
}
