using Overview.Client.Infrastructure.Persistence.Options;
using Overview.Client.Infrastructure.Persistence.Records;
using SQLite;

namespace Overview.Client.Infrastructure.Persistence.Services;

public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly ClientSqliteOptions options;
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private SQLiteAsyncConnection? connection;

    public SqliteConnectionFactory(ClientSqliteOptions? options = null)
    {
        this.options = options ?? new ClientSqliteOptions();
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (connection is not null)
        {
            return connection;
        }

        await initializationLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (connection is not null)
            {
                return connection;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(options.DatabasePath)!);
            connection = new SQLiteAsyncConnection(options.DatabasePath, options.OpenFlags);

            await connection.CreateTableAsync<ItemRecord>().ConfigureAwait(false);
            await connection.CreateTableAsync<UserSettingsRecord>().ConfigureAwait(false);
            await connection.CreateTableAsync<AiChatMessageRecord>().ConfigureAwait(false);
            await connection.CreateTableAsync<SyncChangeRecord>().ConfigureAwait(false);

            return connection;
        }
        finally
        {
            initializationLock.Release();
        }
    }
}
