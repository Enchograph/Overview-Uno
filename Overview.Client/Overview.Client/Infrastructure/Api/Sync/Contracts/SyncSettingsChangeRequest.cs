using Overview.Client.Domain.Entities;

namespace Overview.Client.Infrastructure.Api.Sync.Contracts;

public sealed class SyncSettingsChangeRequest
{
    public Guid ChangeId { get; init; }

    public UserSettings Value { get; init; } = new();

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset LastModifiedAt { get; init; }
}
