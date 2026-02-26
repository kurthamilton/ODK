using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ODK.Core.Utils;

public static class JsonUtils
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, DefaultOptions);

    public static JsonNode? Find(this JsonNode? node, Func<JsonNodeContext, bool> predicate)
    {
        var context = new JsonNodeContext
        {
            Node = node,
            ParentContext = null,
            PropertyName = null
        };

        var result = Find(context, predicate);

        return result?.Node;
    }

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, DefaultOptions);

    public static bool TryDeserialize<T>(string json, [NotNullWhen(true)] out T? result)
    {
        try
        {
            var deserialized = Deserialize<T>(json);
            result = deserialized;
            return result != null;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    private static JsonNodeContext? Find(JsonNodeContext context, Func<JsonNodeContext, bool> predicate)
    {
        var node = context.Node;
        if (predicate(context))
        {
            return context;
        }

        IEnumerable<JsonNodeContext>? contexts = null;

        if (node is JsonArray jsonArray)
        {
            contexts = jsonArray
                .Select(x => new JsonNodeContext
                {
                    Node = x,
                    ParentContext = context,
                    PropertyName = null
                });
        }

        if (node is JsonObject jsonObject)
        {
            contexts = jsonObject
                .Select(x => new JsonNodeContext
                {
                    Node = x.Value,
                    ParentContext = context,
                    PropertyName = x.Key
                });
        }

        return contexts
            ?.Select(x => Find(x, predicate))
            .FirstOrDefault(x => x != null);
    }
}