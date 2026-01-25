using System.Text.Json.Nodes;

namespace ODK.Core.Utils;

public class JsonNodeContext
{
    public required JsonNode? Node { get; init; }

    public required JsonNodeContext? ParentContext { get; init; }

    public required string? PropertyName { get; init; }
}
