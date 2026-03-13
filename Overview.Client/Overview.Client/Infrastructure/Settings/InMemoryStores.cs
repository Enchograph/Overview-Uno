using System.Collections.Concurrent;
using Overview.Client.Application.Auth;
using Overview.Client.Application.Sync;

namespace Overview.Client.Infrastructure.Settings;

public sealed class InMemoryAuthSessionStore : IAuthSessionStore
{
    private AuthSession? session;

    public Task<AuthSession?> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(session);
    }

    public Task SaveAsync(AuthSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();
        this.session = session;
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        session = null;
        return Task.CompletedTask;
    }
}

public sealed class InMemorySyncStateStore : ISyncStateStore
{
    private readonly ConcurrentDictionary<Guid, SyncCheckpoint> checkpoints = new();

    public Task<SyncCheckpoint?> LoadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        checkpoints.TryGetValue(userId, out var checkpoint);
        return Task.FromResult(checkpoint);
    }

    public Task SaveAsync(SyncCheckpoint checkpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        cancellationToken.ThrowIfCancellationRequested();
        checkpoints[checkpoint.UserId] = checkpoint;
        return Task.CompletedTask;
    }

    public Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        checkpoints.TryRemove(userId, out _);
        return Task.CompletedTask;
    }
}

public sealed class InMemoryDeviceIdStore : IDeviceIdStore
{
    private readonly string deviceId = $"device-{Guid.NewGuid():N}";

    public Task<string> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(deviceId);
    }
}
