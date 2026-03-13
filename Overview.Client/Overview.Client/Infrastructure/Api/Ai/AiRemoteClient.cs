using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Overview.Client.Application.Ai;

namespace Overview.Client.Infrastructure.Api.Ai;

public sealed class AiRemoteClient : IAiRemoteClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient httpClient;

    public AiRemoteClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> CompleteChatAsync(
        string baseUrl,
        string apiKey,
        AiChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentNullException.ThrowIfNull(request);

        var endpoint = new Uri(new Uri(AppendTrailingSlash(baseUrl), UriKind.Absolute), "chat/completions");
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(request, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"AI request failed with status {(int)response.StatusCode}: {responseContent}");
        }

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(responseContent);
        if (!document.RootElement.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("AI response does not contain any choices.");
        }

        var choice = choices[0];
        if (!choice.TryGetProperty("message", out var messageElement))
        {
            throw new InvalidOperationException("AI response choice is missing a message payload.");
        }

        if (!messageElement.TryGetProperty("content", out var contentElement))
        {
            return string.Empty;
        }

        return ReadContent(contentElement);
    }

    private static string ReadContent(JsonElement contentElement)
    {
        return contentElement.ValueKind switch
        {
            JsonValueKind.String => contentElement.GetString() ?? string.Empty,
            JsonValueKind.Array => string.Join(
                Environment.NewLine,
                contentElement.EnumerateArray()
                    .Select(part => part.TryGetProperty("text", out var text) ? text.GetString() : null)
                    .Where(text => !string.IsNullOrWhiteSpace(text))),
            _ => string.Empty
        };
    }

    private static string AppendTrailingSlash(string baseUrl)
    {
        var trimmed = baseUrl.Trim();
        return trimmed.EndsWith("/", StringComparison.Ordinal) ? trimmed : $"{trimmed}/";
    }
}
