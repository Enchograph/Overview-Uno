using Overview.Client.Domain.Entities;

namespace Overview.Client.Infrastructure.Api.Sync.Contracts;

public sealed class SyncItemContract
{
    public Item Value { get; init; } = new();
}
