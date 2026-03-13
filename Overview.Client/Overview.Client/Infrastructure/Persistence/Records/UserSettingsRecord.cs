using SQLite;

namespace Overview.Client.Infrastructure.Persistence.Records;

[Table("user_settings")]
internal sealed class UserSettingsRecord
{
    [PrimaryKey]
    public string UserId { get; set; } = string.Empty;

    [Indexed]
    public long LastModifiedAtTicks { get; set; }

    public string PayloadJson { get; set; } = string.Empty;
}
