using System.Text.Json;
using System.Text.Json.Serialization;
using Overview.Client.Application.Auth;

namespace Overview.Client.Infrastructure.Settings;

public sealed class FileAuthSessionStore : IAuthSessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string sessionFilePath;
    private readonly SemaphoreSlim fileLock = new(1, 1);

    public FileAuthSessionStore(string? sessionFilePath = null)
    {
        this.sessionFilePath = sessionFilePath ?? BuildDefaultPath();
    }

    public async Task<AuthSession?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!File.Exists(sessionFilePath))
            {
                return null;
            }

            await using var stream = File.OpenRead(sessionFilePath);
            return await JsonSerializer.DeserializeAsync<AuthSession>(stream, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task SaveAsync(AuthSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(sessionFilePath)!);
            await using var stream = File.Create(sessionFilePath);
            await JsonSerializer.SerializeAsync(stream, session, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (File.Exists(sessionFilePath))
            {
                File.Delete(sessionFilePath);
            }
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

        return Path.Combine(directory, "auth-session.json");
    }
}
