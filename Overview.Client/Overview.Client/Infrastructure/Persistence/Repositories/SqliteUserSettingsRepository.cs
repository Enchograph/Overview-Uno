using Overview.Client.Domain.Entities;
using Overview.Client.Infrastructure.Persistence.Records;
using Overview.Client.Infrastructure.Persistence.Services;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public sealed class SqliteUserSettingsRepository : IUserSettingsRepository
{
    private readonly ISqliteConnectionFactory connectionFactory;

    public SqliteUserSettingsRepository(ISqliteConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task<UserSettings?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        var record = await connection.Table<UserSettingsRecord>()
            .Where(row => row.UserId == userId.ToString())
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return record is null ? null : ClientJsonSerializer.Deserialize<UserSettings>(record.PayloadJson);
    }

    public async Task UpsertAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
        await connection.InsertOrReplaceAsync(new UserSettingsRecord
        {
            UserId = settings.UserId.ToString(),
            LastModifiedAtTicks = settings.LastModifiedAt.UtcTicks,
            PayloadJson = ClientJsonSerializer.Serialize(settings),
        }).ConfigureAwait(false);
    }
}
