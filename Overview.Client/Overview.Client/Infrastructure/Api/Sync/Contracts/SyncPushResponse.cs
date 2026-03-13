namespace Overview.Client.Infrastructure.Api.Sync.Contracts;

public sealed class SyncPushResponse
{
    public bool Accepted { get; init; }

    public DateTimeOffset ServerTime { get; init; }

    public int AppliedChangeCount { get; init; }

    public IReadOnlyList<SyncConflictContract> Conflicts { get; init; } = [];
}
