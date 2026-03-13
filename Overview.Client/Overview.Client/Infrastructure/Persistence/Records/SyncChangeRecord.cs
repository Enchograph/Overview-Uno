using SQLite;

namespace Overview.Client.Infrastructure.Persistence.Records;

[Table("sync_changes")]
internal sealed class SyncChangeRecord
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    [Indexed]
    public long LastModifiedAtTicks { get; set; }

    [Indexed]
    public long? SyncedAtTicks { get; set; }

    [MaxLength(64)]
    public string EntityType { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;
}
