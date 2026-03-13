using Overview.Server.Domain.Entities;

namespace Overview.Server.Api.Contracts.Sync;

public sealed class SyncItemContract
{
    public Item Value { get; init; } = new();
}
