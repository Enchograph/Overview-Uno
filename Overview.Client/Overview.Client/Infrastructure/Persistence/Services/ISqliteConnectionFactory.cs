using SQLite;

namespace Overview.Client.Infrastructure.Persistence.Services;

public interface ISqliteConnectionFactory
{
    Task<SQLiteAsyncConnection> GetConnectionAsync();
}
