using System.Text.Json;

namespace ODK.Core.Utils;

public static class JsonUtils
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, DefaultOptions);

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, DefaultOptions);

    public static bool TryDeserialize<T>(string json, out T? result)
    {
        try
        {
            var deserialized = Deserialize<T>(json);
            result = deserialized;
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}