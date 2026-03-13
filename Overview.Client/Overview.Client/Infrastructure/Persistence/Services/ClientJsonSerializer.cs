using System.Text.Json;
using System.Text.Json.Serialization;

namespace Overview.Client.Infrastructure.Persistence.Services;

internal static class ClientJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    public static TValue Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, Options)
            ?? throw new InvalidOperationException($"Unable to deserialize {typeof(TValue).FullName}.");
    }
}
