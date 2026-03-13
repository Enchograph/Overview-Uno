using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Presentation.ViewModels;

public sealed class AddItemListEntry
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required ItemType Type { get; init; }

    public required bool IsCompleted { get; init; }

    public required DateTimeOffset LastModifiedAt { get; init; }

    public string Subtitle => $"{Type} · {(IsCompleted ? "Completed" : "Active")} · {LastModifiedAt.LocalDateTime:yyyy-MM-dd HH:mm}";

    public static AddItemListEntry FromItem(Item item)
    {
        return new AddItemListEntry
        {
            Id = item.Id,
            Title = item.Title,
            Type = item.Type,
            IsCompleted = item.IsCompleted,
            LastModifiedAt = item.LastModifiedAt
        };
    }
}
