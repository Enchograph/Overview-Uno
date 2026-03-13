namespace Overview.Server.Infrastructure.Configuration;

public sealed class SyncOptions
{
    public const string SectionName = "Sync";

    public int MaxBatchSize { get; init; } = 200;

    public string ConflictPolicy { get; init; } = "LastModifiedWins";
}
