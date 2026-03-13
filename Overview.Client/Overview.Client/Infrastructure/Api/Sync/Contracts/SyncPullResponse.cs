namespace Overview.Client.Infrastructure.Api.Sync.Contracts;

public sealed class SyncPullResponse
{
    public DateTimeOffset ServerTime { get; init; }

    public DateTimeOffset? Since { get; init; }

    public IReadOnlyList<SyncItemContract> Items { get; init; } = [];

    public SyncSettingsContract? Settings { get; init; }
}
