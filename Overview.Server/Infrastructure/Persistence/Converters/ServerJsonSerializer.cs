using System.Text.Json;
using System.Text.Json.Serialization;

namespace Overview.Server.Infrastructure.Persistence.Converters;

internal static class ServerJsonSerializer
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
