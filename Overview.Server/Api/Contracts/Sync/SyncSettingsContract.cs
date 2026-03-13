using Overview.Server.Domain.Entities;

namespace Overview.Server.Api.Contracts.Sync;

public sealed class SyncSettingsContract
{
    public UserSettings Value { get; init; } = new();
}
