using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Presentation.ViewModels;

public sealed class AiChatMessageEntryViewModel
{
    public Guid MessageId { get; init; }

    public AiChatRole Role { get; init; }

    public string SpeakerLabel { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string TimestampLabel { get; init; } = string.Empty;

    public static AiChatMessageEntryViewModel FromMessage(AiChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new AiChatMessageEntryViewModel
        {
            MessageId = message.Id,
            Role = message.Role,
            SpeakerLabel = message.Role switch
            {
                AiChatRole.User => "You",
                AiChatRole.Assistant => "AI",
                _ => "System"
            },
            Message = message.Message,
            TimestampLabel = message.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
        };
    }
}
