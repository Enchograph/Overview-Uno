using Overview.Server.Domain.Entities;
using Overview.Server.Domain.Enums;

namespace Overview.Server.Api.Contracts.Sync;

public sealed class SyncItemChangeRequest
{
    public Guid ChangeId { get; init; }

    public SyncChangeType ChangeType { get; init; } = SyncChangeType.Upsert;

    public Guid? EntityId { get; init; }

    public Item? Item { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset LastModifiedAt { get; init; }
}
