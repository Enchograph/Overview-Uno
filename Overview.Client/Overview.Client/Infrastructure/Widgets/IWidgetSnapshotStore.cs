using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

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

public sealed class FileWidgetSnapshotStore : IWidgetSnapshotStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string snapshotFilePath;
    private readonly SemaphoreSlim fileLock = new(1, 1);

    public FileWidgetSnapshotStore(string? snapshotFilePath = null)
    {
        this.snapshotFilePath = snapshotFilePath ?? BuildDefaultPath();
    }

    public async Task SaveAsync(WidgetSnapshot snapshot, CancellationToken cancellationToken)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var snapshots = await LoadAllSnapshotsCoreAsync(cancellationToken).ConfigureAwait(false);
            snapshots[snapshot.Kind] = snapshot;
            Directory.CreateDirectory(Path.GetDirectoryName(snapshotFilePath)!);
            await using var stream = File.Create(snapshotFilePath);
            await JsonSerializer.SerializeAsync(stream, snapshots, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task<WidgetSnapshot?> GetAsync(WidgetKind kind, CancellationToken cancellationToken)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var snapshots = await LoadAllSnapshotsCoreAsync(cancellationToken).ConfigureAwait(false);
            snapshots.TryGetValue(kind, out var snapshot);
            return snapshot;
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task RemoveAsync(WidgetKind kind, CancellationToken cancellationToken)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var snapshots = await LoadAllSnapshotsCoreAsync(cancellationToken).ConfigureAwait(false);
            if (!snapshots.Remove(kind))
            {
                return;
            }

            if (snapshots.Count == 0)
            {
                if (File.Exists(snapshotFilePath))
                {
                    File.Delete(snapshotFilePath);
                }

                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(snapshotFilePath)!);
            await using var stream = File.Create(snapshotFilePath);
            await JsonSerializer.SerializeAsync(stream, snapshots, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private async Task<Dictionary<WidgetKind, WidgetSnapshot>> LoadAllSnapshotsCoreAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(snapshotFilePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(snapshotFilePath);
        return await JsonSerializer.DeserializeAsync<Dictionary<WidgetKind, WidgetSnapshot>>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false)
            ?? [];
    }

    private static string BuildDefaultPath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Overview.Client");

        return Path.Combine(directory, "widget-snapshots.json");
    }
}
