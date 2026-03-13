using SQLite;

namespace Overview.Client.Infrastructure.Persistence.Options;

public sealed class ClientSqliteOptions
{
    public string DatabaseName { get; init; } = "overview.db3";

    public SQLiteOpenFlags OpenFlags { get; init; } =
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.SharedCache;

    public string DatabasePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabaseName);
}
