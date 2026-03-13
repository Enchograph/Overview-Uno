namespace Overview.Server.Infrastructure.Configuration;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string Provider { get; init; } = "PostgreSQL";

    public string ConnectionStringName { get; init; } = "DefaultConnection";
}
