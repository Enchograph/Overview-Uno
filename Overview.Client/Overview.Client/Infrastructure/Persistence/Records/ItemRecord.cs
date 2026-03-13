using SQLite;

namespace Overview.Client.Infrastructure.Persistence.Records;

[Table("items")]
internal sealed class ItemRecord
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    [Indexed]
    public long LastModifiedAtTicks { get; set; }

    [Indexed]
    public bool IsDeleted { get; set; }

    [MaxLength(64)]
    public string ItemType { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;
}
