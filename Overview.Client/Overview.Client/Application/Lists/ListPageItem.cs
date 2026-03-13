using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Lists;

public sealed record ListPageItem
{
    public Guid ItemId { get; init; }

    public ItemType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public bool IsImportant { get; init; }

    public bool IsCompleted { get; init; }

    public DateOnly? RelevantDate { get; init; }

    public DateOnly? DeadlineDate { get; init; }

    public DateTimeOffset LastModifiedAt { get; init; }
}
