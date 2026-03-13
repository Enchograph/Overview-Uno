using SQLite;

namespace Overview.Client.Infrastructure.Persistence.Records;

[Table("ai_chat_messages")]
internal sealed class AiChatMessageRecord
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    [Indexed]
    public string OccurredOn { get; set; } = string.Empty;

    [Indexed]
    public long CreatedAtTicks { get; set; }

    public string PayloadJson { get; set; } = string.Empty;
}
