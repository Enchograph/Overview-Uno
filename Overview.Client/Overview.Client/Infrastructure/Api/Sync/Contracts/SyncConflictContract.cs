using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Infrastructure.Api.Sync.Contracts;

public sealed class SyncConflictContract
{
    public Guid ChangeId { get; init; }

    public SyncEntityType EntityType { get; init; } = SyncEntityType.Item;

    public Guid? EntityId { get; init; }

    public string Reason { get; init; } = string.Empty;

    public DateTimeOffset ServerLastModifiedAt { get; init; }

    public Item? ServerItem { get; init; }

    public UserSettings? ServerSettings { get; init; }
}
