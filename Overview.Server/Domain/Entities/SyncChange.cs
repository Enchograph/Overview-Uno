using Overview.Server.Domain.Enums;

namespace Overview.Server.Domain.Entities;

public sealed class SyncChange
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid UserId { get; init; }

    public string DeviceId { get; init; } = string.Empty;

    public SyncEntityType EntityType { get; init; } = SyncEntityType.Item;

    public SyncChangeType ChangeType { get; init; } = SyncChangeType.Upsert;

    public Guid? EntityId { get; init; }

    public Item? ItemSnapshot { get; init; }

    public UserSettings? SettingsSnapshot { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset LastModifiedAt { get; init; }

    public DateTimeOffset? SyncedAt { get; init; }
}
