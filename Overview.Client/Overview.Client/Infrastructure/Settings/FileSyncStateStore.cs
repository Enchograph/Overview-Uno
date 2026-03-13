using System.Text.Json;
using System.Text.Json.Serialization;
using Overview.Client.Application.Sync;

namespace Overview.Client.Infrastructure.Settings;

public sealed class FileSyncStateStore : ISyncStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string stateFilePath;
    private readonly SemaphoreSlim fileLock = new(1, 1);

    public FileSyncStateStore(string? stateFilePath = null)
    {
        this.stateFilePath = stateFilePath ?? BuildDefaultPath();
    }

    public async Task<SyncCheckpoint?> LoadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var allStates = await LoadAllStatesCoreAsync(cancellationToken).ConfigureAwait(false);
            return allStates.TryGetValue(userId, out var checkpoint) ? checkpoint : null;
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task SaveAsync(SyncCheckpoint checkpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);

        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var allStates = await LoadAllStatesCoreAsync(cancellationToken).ConfigureAwait(false);
            allStates[checkpoint.UserId] = checkpoint;
            Directory.CreateDirectory(Path.GetDirectoryName(stateFilePath)!);
            await using var stream = File.Create(stateFilePath);
            await JsonSerializer.SerializeAsync(stream, allStates, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var allStates = await LoadAllStatesCoreAsync(cancellationToken).ConfigureAwait(false);
            if (!allStates.Remove(userId))
            {
                return;
            }

            if (allStates.Count == 0)
            {
                if (File.Exists(stateFilePath))
                {
                    File.Delete(stateFilePath);
                }

                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(stateFilePath)!);
            await using var stream = File.Create(stateFilePath);
            await JsonSerializer.SerializeAsync(stream, allStates, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private async Task<Dictionary<Guid, SyncCheckpoint>> LoadAllStatesCoreAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(stateFilePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(stateFilePath);
        return await JsonSerializer.DeserializeAsync<Dictionary<Guid, SyncCheckpoint>>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false)
            ?? [];
    }

    private static string BuildDefaultPath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Overview.Client");

        return Path.Combine(directory, "sync-state.json");
    }
}
