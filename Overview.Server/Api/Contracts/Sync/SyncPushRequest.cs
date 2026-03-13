namespace Overview.Server.Api.Contracts.Sync;

public sealed class SyncPushRequest
{
    public string DeviceId { get; init; } = string.Empty;

    public DateTimeOffset? LastKnownServerTime { get; init; }

    public IReadOnlyList<SyncItemChangeRequest> ItemChanges { get; init; } = [];

    public SyncSettingsChangeRequest? SettingsChange { get; init; }
}
