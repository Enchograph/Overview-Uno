using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Overview.Client.Application.Auth;
using Overview.Client.Infrastructure.Api.Auth.Contracts;

namespace Overview.Client.Infrastructure.Api.Auth;

public sealed class AuthRemoteClient : IAuthRemoteClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient httpClient;

    public AuthRemoteClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<VerificationCodeDispatchResult> SendVerificationCodeAsync(
        string baseUrl,
        string email,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<AuthSendVerificationCodeRequest, AuthSendVerificationCodeResponse>(
            baseUrl,
            "api/auth/send-verification-code",
            new AuthSendVerificationCodeRequest
            {
                Email = email
            },
            cancellationToken).ConfigureAwait(false);

        return new VerificationCodeDispatchResult
        {
            Email = response.Email,
            ExpiresAt = response.ExpiresAt
        };
    }

    public Task<AuthTokenResult> RegisterAsync(
        string baseUrl,
        string email,
        string password,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<AuthRegisterRequest, AuthTokenResult>(
            baseUrl,
            "api/auth/register",
            new AuthRegisterRequest
            {
                Email = email,
                Password = password,
                VerificationCode = verificationCode
            },
            cancellationToken);
    }

    public Task<AuthTokenResult> LoginAsync(
        string baseUrl,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<AuthLoginRequest, AuthTokenResult>(
            baseUrl,
            "api/auth/login",
            new AuthLoginRequest
            {
                Email = email,
                Password = password
            },
            cancellationToken);
    }

    public Task<AuthTokenResult> RefreshAsync(
        string baseUrl,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<AuthRefreshRequest, AuthTokenResult>(
            baseUrl,
            "api/auth/refresh",
            new AuthRefreshRequest
            {
                RefreshToken = refreshToken
            },
            cancellationToken);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string baseUrl,
        string relativePath,
        TRequest payload,
        CancellationToken cancellationToken)
    {
        var endpoint = new Uri(new Uri(AppendTrailingSlash(baseUrl), UriKind.Absolute), relativePath);
        using var response = await httpClient.PostAsJsonAsync(endpoint, payload, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new AuthRemoteException(
                $"Authentication request failed with status {(int)response.StatusCode}.",
                response.StatusCode,
                errorBody);
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Unable to deserialize {typeof(TResponse).FullName}.");
    }

    private static string AppendTrailingSlash(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL is required.", nameof(baseUrl));
        }

        var trimmed = baseUrl.Trim();
        return trimmed.EndsWith("/", StringComparison.Ordinal) ? trimmed : $"{trimmed}/";
    }
}
