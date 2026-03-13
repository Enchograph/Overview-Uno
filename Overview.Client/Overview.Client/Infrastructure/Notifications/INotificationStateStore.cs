using System.Text.Json;
using System.Text.Json.Serialization;

namespace Overview.Client.Infrastructure.Notifications;

public interface INotificationStateStore
{
    Task<IReadOnlyCollection<string>> LoadAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SaveAsync(Guid userId, IReadOnlyCollection<string> notificationIds, CancellationToken cancellationToken = default);

    Task ClearAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed class InMemoryNotificationStateStore : INotificationStateStore
{
    private readonly Dictionary<Guid, IReadOnlyCollection<string>> notificationIdsByUser = [];

    public Task<IReadOnlyCollection<string>> LoadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            notificationIdsByUser.TryGetValue(userId, out var notificationIds)
                ? notificationIds
                : (IReadOnlyCollection<string>)Array.Empty<string>());
    }

    public Task SaveAsync(Guid userId, IReadOnlyCollection<string> notificationIds, CancellationToken cancellationToken = default)
    {
        notificationIdsByUser[userId] = notificationIds.ToArray();
        return Task.CompletedTask;
    }

    public Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        notificationIdsByUser.Remove(userId);
        return Task.CompletedTask;
    }
}

public sealed class FileNotificationStateStore : INotificationStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string stateFilePath;
    private readonly SemaphoreSlim fileLock = new(1, 1);

    public FileNotificationStateStore(string? stateFilePath = null)
    {
        this.stateFilePath = stateFilePath ?? BuildDefaultPath();
    }

    public async Task<IReadOnlyCollection<string>> LoadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var allStates = await LoadAllStatesCoreAsync(cancellationToken).ConfigureAwait(false);
            return allStates.TryGetValue(userId, out var notificationIds)
                ? notificationIds
                : Array.Empty<string>();
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task SaveAsync(Guid userId, IReadOnlyCollection<string> notificationIds, CancellationToken cancellationToken = default)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var allStates = await LoadAllStatesCoreAsync(cancellationToken).ConfigureAwait(false);
            allStates[userId] = notificationIds.Distinct(StringComparer.Ordinal).ToArray();
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

    private async Task<Dictionary<Guid, string[]>> LoadAllStatesCoreAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(stateFilePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(stateFilePath);
        return await JsonSerializer.DeserializeAsync<Dictionary<Guid, string[]>>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false)
            ?? [];
    }

    private static string BuildDefaultPath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Overview.Client");

        return Path.Combine(directory, "notification-state.json");
    }
}
