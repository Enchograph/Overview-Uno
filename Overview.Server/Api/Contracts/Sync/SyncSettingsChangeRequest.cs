using Overview.Server.Domain.Entities;

namespace Overview.Server.Api.Contracts.Sync;

public sealed class SyncSettingsChangeRequest
{
    public Guid ChangeId { get; init; }

    public UserSettings Value { get; init; } = new();

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset LastModifiedAt { get; init; }
}
