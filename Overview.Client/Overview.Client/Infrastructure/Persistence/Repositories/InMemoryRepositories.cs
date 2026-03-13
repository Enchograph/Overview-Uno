using System.Collections.Concurrent;
using Overview.Client.Domain.Entities;
using Overview.Client.Infrastructure.Persistence.Services;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public sealed class InMemoryItemRepository : IItemRepository
{
    private readonly ConcurrentDictionary<(Guid UserId, Guid ItemId), string> items = new();

    public Task<Item?> GetAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(items.TryGetValue((userId, itemId), out var payload)
            ? ClientJsonSerializer.Deserialize<Item>(payload)
            : null);
    }

    public Task<IReadOnlyList<Item>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = items
            .Where(entry => entry.Key.UserId == userId)
            .Select(entry => ClientJsonSerializer.Deserialize<Item>(entry.Value))
            .OrderByDescending(item => item.LastModifiedAt.UtcTicks)
            .ToArray();

        return Task.FromResult<IReadOnlyList<Item>>(results);
    }

    public Task UpsertAsync(Item item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        cancellationToken.ThrowIfCancellationRequested();
        items[(item.UserId, item.Id)] = ClientJsonSerializer.Serialize(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        items.TryRemove((userId, itemId), out _);
        return Task.CompletedTask;
    }
}

public sealed class InMemoryUserSettingsRepository : IUserSettingsRepository
{
    private readonly ConcurrentDictionary<Guid, string> settingsByUser = new();

    public Task<UserSettings?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(settingsByUser.TryGetValue(userId, out var payload)
            ? ClientJsonSerializer.Deserialize<UserSettings>(payload)
            : null);
    }

    public Task UpsertAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();
        settingsByUser[settings.UserId] = ClientJsonSerializer.Serialize(settings);
        return Task.CompletedTask;
    }
}

public sealed class InMemoryAiChatMessageRepository : IAiChatMessageRepository
{
    private readonly ConcurrentDictionary<(Guid UserId, Guid MessageId), string> messages = new();

    public Task<IReadOnlyList<AiChatMessage>> ListByDateRangeAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = messages
            .Where(entry => entry.Key.UserId == userId)
            .Select(entry => ClientJsonSerializer.Deserialize<AiChatMessage>(entry.Value))
            .Where(message => message.OccurredOn >= startDate && message.OccurredOn <= endDate)
            .OrderBy(message => message.CreatedAt)
            .ToArray();

        return Task.FromResult<IReadOnlyList<AiChatMessage>>(results);
    }

    public Task UpsertAsync(AiChatMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        messages[(message.UserId, message.Id)] = ClientJsonSerializer.Serialize(message);
        return Task.CompletedTask;
    }
}

public sealed class InMemorySyncChangeRepository : ISyncChangeRepository
{
    private readonly ConcurrentDictionary<(Guid UserId, Guid ChangeId), string> changes = new();

    public Task<IReadOnlyList<SyncChange>> ListPendingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = changes
            .Where(entry => entry.Key.UserId == userId)
            .Select(entry => ClientJsonSerializer.Deserialize<SyncChange>(entry.Value))
            .Where(change => change.SyncedAt is null)
            .OrderBy(change => change.CreatedAt)
            .ToArray();

        return Task.FromResult<IReadOnlyList<SyncChange>>(results);
    }

    public Task UpsertAsync(SyncChange change, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(change);
        cancellationToken.ThrowIfCancellationRequested();
        changes[(change.UserId, change.Id)] = ClientJsonSerializer.Serialize(change);
        return Task.CompletedTask;
    }

    public Task MarkSyncedAsync(
        IEnumerable<Guid> changeIds,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var key in changes.Keys.Where(key => changeIds.Contains(key.ChangeId)).ToArray())
        {
            var change = ClientJsonSerializer.Deserialize<SyncChange>(changes[key]);
            var syncedChange = new SyncChange
            {
                Id = change.Id,
                UserId = change.UserId,
                DeviceId = change.DeviceId,
                EntityType = change.EntityType,
                ChangeType = change.ChangeType,
                EntityId = change.EntityId,
                ItemSnapshot = change.ItemSnapshot,
                SettingsSnapshot = change.SettingsSnapshot,
                CreatedAt = change.CreatedAt,
                LastModifiedAt = change.LastModifiedAt,
                SyncedAt = syncedAt
            };
            changes[key] = ClientJsonSerializer.Serialize(syncedChange);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(IEnumerable<Guid> changeIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var key in changes.Keys.Where(key => changeIds.Contains(key.ChangeId)).ToArray())
        {
            changes.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
