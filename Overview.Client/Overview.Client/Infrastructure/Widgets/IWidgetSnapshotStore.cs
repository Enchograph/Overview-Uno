using System.Collections.Concurrent;

namespace Overview.Client.Infrastructure.Widgets;

public interface IWidgetSnapshotStore
{
    Task SaveAsync(WidgetSnapshot snapshot, CancellationToken cancellationToken);

    Task<WidgetSnapshot?> GetAsync(WidgetKind kind, CancellationToken cancellationToken);

    Task RemoveAsync(WidgetKind kind, CancellationToken cancellationToken);
}

public enum WidgetKind
{
    Home = 0,
    List = 1,
    AiShortcut = 2,
    QuickAdd = 3
}

public sealed record WidgetSnapshot
{
    public WidgetKind Kind { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Subtitle { get; init; }

    public string? DeepLink { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public IReadOnlyList<WidgetSnapshotEntry> Entries { get; init; } = Array.Empty<WidgetSnapshotEntry>();
}

public sealed record WidgetSnapshotEntry
{
    public string Title { get; init; } = string.Empty;

    public string? Subtitle { get; init; }

    public string? BadgeText { get; init; }

    public string? AccentColor { get; init; }
}

public sealed class InMemoryWidgetSnapshotStore : IWidgetSnapshotStore
{
    private readonly ConcurrentDictionary<WidgetKind, WidgetSnapshot> _snapshots = new();

    public Task SaveAsync(WidgetSnapshot snapshot, CancellationToken cancellationToken)
    {
        _snapshots[snapshot.Kind] = snapshot;
        return Task.CompletedTask;
    }

    public Task<WidgetSnapshot?> GetAsync(WidgetKind kind, CancellationToken cancellationToken)
    {
        _snapshots.TryGetValue(kind, out var snapshot);
        return Task.FromResult(snapshot);
    }

    public Task RemoveAsync(WidgetKind kind, CancellationToken cancellationToken)
    {
        _snapshots.TryRemove(kind, out _);
        return Task.CompletedTask;
    }
}
