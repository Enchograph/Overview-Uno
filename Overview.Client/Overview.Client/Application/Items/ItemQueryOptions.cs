using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Items;

public sealed record ItemQueryOptions
{
    public bool IncludeDeleted { get; init; }

    public ItemType? Type { get; init; }
}
