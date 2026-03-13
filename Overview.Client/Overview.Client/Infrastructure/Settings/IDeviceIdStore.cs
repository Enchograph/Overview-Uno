namespace Overview.Client.Infrastructure.Settings;

public interface IDeviceIdStore
{
    Task<string> GetOrCreateAsync(CancellationToken cancellationToken = default);
}
