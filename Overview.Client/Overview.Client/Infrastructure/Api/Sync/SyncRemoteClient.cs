using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Overview.Client.Infrastructure.Api.Sync.Contracts;

namespace Overview.Client.Infrastructure.Api.Sync;

public sealed class SyncRemoteClient : ISyncRemoteClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;

    public SyncRemoteClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<SyncPullResponse> PullAsync(
        string baseUrl,
        string accessToken,
        DateTimeOffset? since,
        CancellationToken cancellationToken = default)
    {
        var path = since is null
            ? "api/sync/pull"
            : $"api/sync/pull?since={Uri.EscapeDataString(since.Value.ToString("O"))}";

        using var request = CreateRequestMessage(HttpMethod.Get, baseUrl, path, accessToken);
        return SendAsync<SyncPullResponse>(request, cancellationToken);
    }

    public Task<SyncPushResponse> PushAsync(
        string baseUrl,
        string accessToken,
        SyncPushRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = CreateRequestMessage(HttpMethod.Post, baseUrl, "api/sync/push", accessToken);
        var payload = JsonSerializer.Serialize(request, JsonOptions);
        message.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        return SendAsync<SyncPushResponse>(message, cancellationToken);
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Sync request failed with status {(int)response.StatusCode}: {body}",
                null,
                response.StatusCode);
        }

        return JsonSerializer.Deserialize<TResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException($"Unable to deserialize {typeof(TResponse).FullName}.");
    }

    private static HttpRequestMessage CreateRequestMessage(
        HttpMethod method,
        string baseUrl,
        string relativePath,
        string accessToken)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL is required.", nameof(baseUrl));
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Access token is required.", nameof(accessToken));
        }

        var endpoint = new Uri(new Uri(AppendTrailingSlash(baseUrl), UriKind.Absolute), relativePath);
        var request = new HttpRequestMessage(method, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static string AppendTrailingSlash(string baseUrl)
    {
        return baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : $"{baseUrl}/";
    }
}
