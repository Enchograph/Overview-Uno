using Overview.Client.Domain.Entities;

namespace Overview.Client.Infrastructure.Api.Sync.Contracts;

public sealed class SyncSettingsContract
{
    public UserSettings Value { get; init; } = new();
}
