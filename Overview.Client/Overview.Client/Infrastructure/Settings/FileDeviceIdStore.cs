namespace Overview.Client.Infrastructure.Settings;

public sealed class FileDeviceIdStore : IDeviceIdStore
{
    private readonly string deviceIdFilePath;
    private readonly SemaphoreSlim fileLock = new(1, 1);

    public FileDeviceIdStore(string? deviceIdFilePath = null)
    {
        this.deviceIdFilePath = deviceIdFilePath ?? BuildDefaultPath();
    }

    public async Task<string> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (File.Exists(deviceIdFilePath))
            {
                var deviceId = (await File.ReadAllTextAsync(deviceIdFilePath, cancellationToken).ConfigureAwait(false)).Trim();
                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    return deviceId;
                }
            }

            var createdDeviceId = $"device-{Guid.NewGuid():N}";
            Directory.CreateDirectory(Path.GetDirectoryName(deviceIdFilePath)!);
            await File.WriteAllTextAsync(deviceIdFilePath, createdDeviceId, cancellationToken).ConfigureAwait(false);
            return createdDeviceId;
        }
        finally
        {
            fileLock.Release();
        }
    }

    private static string BuildDefaultPath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Overview.Client");

        return Path.Combine(directory, "device-id.txt");
    }
}
