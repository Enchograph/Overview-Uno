using Overview.Server.Domain.Entities;
using Overview.Server.Domain.Enums;

namespace Overview.Server.Api.Contracts.Sync;

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
