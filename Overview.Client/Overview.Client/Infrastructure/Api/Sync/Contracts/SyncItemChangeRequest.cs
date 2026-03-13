using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Infrastructure.Api.Sync.Contracts;

public sealed class SyncItemChangeRequest
{
    public Guid ChangeId { get; init; }

    public SyncChangeType ChangeType { get; init; } = SyncChangeType.Upsert;

    public Guid? EntityId { get; init; }

    public Item? Item { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset LastModifiedAt { get; init; }
}
