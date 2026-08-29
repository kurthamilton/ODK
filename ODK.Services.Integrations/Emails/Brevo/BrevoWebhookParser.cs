using System.Text.Json;
using System.Text.Json.Nodes;
using ODK.Core.Platforms;
using ODK.Services.Emails;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Emails.Brevo;

public class BrevoWebhookParser : IBrevoWebhookParser
{
    private readonly ILoggingService _loggingService;
    private readonly BrevoWebhookParserSettings _settings;

    public BrevoWebhookParser(
        ILoggingService loggingService,
        BrevoWebhookParserSettings settings)
    {
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<BrevoWebhookEvent?> ParseWebhook(string json)
    {
        JsonNode? node;

        try
        {
            node = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            node = null;
        }

        if (node is not JsonObject payload)
        {
            await _loggingService.Error("Error processing Brevo webhook: payload could not be read");
            return null;
        }

        /* Both sides have to name an environment for a mismatch to mean anything. An untagged event is no
           statement rather than a mismatch, so a message sent before the tag existed still has its events
           recorded - Brevo emits opens and clicks for weeks - and a receiver that does not know which
           deployment it is has nothing to compare against. */
        var environment = BrevoEnvironmentTag.Parse(_settings.EnvironmentTagPrefix, ReadTags(payload));
        var bothNamed = environment != EnvironmentType.None && _settings.Environment != EnvironmentType.None;

        if (bothNamed && environment != _settings.Environment)
        {
            // Silent, and before anything else is read: one send from another deployment produces an event
            // per recipient per event type, and logging those is the noise the tag exists to remove.
            return null;
        }

        var eventName = ReadString(payload["event"]);
        var externalId = ReadString(payload["message-id"]);

        if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(externalId))
        {
            await _loggingService.Error(
                $"Error processing Brevo webhook: event {eventName} or messageId {externalId} not found");
            return null;
        }

        return new BrevoWebhookEvent
        {
            EventName = eventName,
            ExternalId = externalId
        };
    }

    private static string? ReadString(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var result) ? result : null;

    /// <summary>
    /// Brevo states a message's tags two ways and populates both, in different shapes: <c>tags</c> is an
    /// array, and <c>tag</c> is a string holding a JSON-encoded array. Both are read rather than one being
    /// preferred, so a payload carrying only one of them is still understood. A tag arriving twice costs
    /// nothing - <see cref="BrevoEnvironmentTag.Parse"/> asks only whether one is there.
    /// </summary>
    private static IReadOnlyCollection<string> ReadTags(JsonObject payload)
    {
        var tags = new List<string>();

        if (payload["tags"] is JsonArray array)
        {
            tags.AddRange(array.Select(ReadString).OfType<string>());
        }

        var tag = ReadString(payload["tag"]);
        if (string.IsNullOrEmpty(tag))
        {
            return tags;
        }

        if (tag.StartsWith('['))
        {
            try
            {
                if (JsonNode.Parse(tag) is JsonArray tagArray)
                {
                    tags.AddRange(tagArray.Select(ReadString).OfType<string>());
                    return tags;
                }
            }
            catch (JsonException)
            {
                // A tag that opens like an array but does not parse as one is the literal tag it is.
            }
        }

        tags.Add(tag);
        return tags;
    }
}
