using Overview.Client.Infrastructure.Api.Sync.Contracts;

namespace Overview.Client.Infrastructure.Api.Sync;

public interface ISyncRemoteClient
{
    Task<SyncPullResponse> PullAsync(
        string baseUrl,
        string accessToken,
        DateTimeOffset? since,
        CancellationToken cancellationToken = default);

    Task<SyncPushResponse> PushAsync(
        string baseUrl,
        string accessToken,
        SyncPushRequest request,
        CancellationToken cancellationToken = default);
}
