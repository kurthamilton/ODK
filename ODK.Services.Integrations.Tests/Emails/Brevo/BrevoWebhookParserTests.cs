using FluentAssertions;
using Moq;
using ODK.Core.Platforms;
using ODK.Services.Integrations.Emails.Brevo;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Tests.Emails.Brevo;

[Parallelizable]
public static class BrevoWebhookParserTests
{
    private const string EventName = "delivered";

    private const string ExternalId = "<202601010000.1@smtp-relay.mailin.fr>";

    private const string Prefix = "tag-prefix-";

    [Test]
    public static async Task ParseWebhook_InvalidJson_ReturnsNull()
    {
        // Act
        var result = await CreateParser().ParseWebhook("not json");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task ParseWebhook_MatchingTag_ReturnsEvent()
    {
        // Arrange
        var json = CreateJson(tags: $"""["{Prefix}Dev"]""");

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert
        result.Should().NotBeNull();
        result!.EventName.Should().Be(EventName);
        result.ExternalId.Should().Be(ExternalId);
    }

    [Test]
    public static async Task ParseWebhook_MissingEventName_ReturnsNull()
    {
        // Arrange
        var json = $$"""{"message-id":"{{ExternalId}}"}""";

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task ParseWebhook_MissingExternalId_ReturnsNull()
    {
        // Arrange
        var json = $$"""{"event":"{{EventName}}"}""";

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task ParseWebhook_NoTag_DoesNotLog()
    {
        // Arrange
        var loggingService = new Mock<ILoggingService>();

        // Act
        await CreateParser(loggingService.Object).ParseWebhook(CreateJson());

        // Assert
        loggingService.Verify(x => x.Error(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public static async Task ParseWebhook_NoTag_ReturnsEvent()
    {
        // Arrange
        var json = CreateJson();

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert - an untagged event is no statement rather than a mismatch, so it is still acted on
        result.Should().NotBeNull();
    }

    [Test]
    public static async Task ParseWebhook_OtherEnvironmentTag_DoesNotLog()
    {
        // Arrange
        var loggingService = new Mock<ILoggingService>();
        var json = CreateJson(tags: $"""["{Prefix}Prod"]""");

        // Act
        await CreateParser(loggingService.Object).ParseWebhook(json);

        // Assert - the discard is silent, because logging it is the noise the tag exists to remove
        loggingService.Verify(x => x.Error(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public static async Task ParseWebhook_OtherEnvironmentTag_ReturnsNull()
    {
        // Arrange
        var json = CreateJson(tags: $"""["{Prefix}Prod"]""");

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task ParseWebhook_PrefixNotConfigured_ReturnsEvent()
    {
        // Arrange
        var json = CreateJson(tags: $"""["{Prefix}Prod"]""");

        // Act
        var result = await CreateParser(prefix: string.Empty).ParseWebhook(json);

        // Assert - with no prefix there is nothing to recognise a tag by, so nothing is discarded
        result.Should().NotBeNull();
    }

    [Test]
    public static async Task ParseWebhook_ReceiverEnvironmentNotSet_ReturnsEvent()
    {
        // Arrange
        var json = CreateJson(tags: $"""["{Prefix}Prod"]""");

        // Act
        var result = await CreateParser(environment: EnvironmentType.None).ParseWebhook(json);

        // Assert - a receiver that does not know which deployment it is has nothing to compare against
        result.Should().NotBeNull();
    }

    [Test]
    public static async Task ParseWebhook_SingleTagAsBareString_MatchesEnvironment()
    {
        // Arrange - the tag naming another deployment, so a match is what discards it
        var json = CreateJson(tag: $"\"{Prefix}Prod\"");

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task ParseWebhook_SingleTagAsEncodedArray_MatchesEnvironment()
    {
        // Arrange - the shape `unsubscribed` and proxy opens carry: an array, JSON-encoded into a string
        var json = CreateJson(tag: $"\"[\\\"{Prefix}Prod\\\"]\"");

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task ParseWebhook_TagInBothFields_MatchesEnvironment()
    {
        // Arrange - the shape a live payload takes: `tags` and `tag` both carry the tag, in their own shapes
        var json = CreateJson(tags: $"""["{Prefix}Prod"]""", tag: $"\"[\\\"{Prefix}Prod\\\"]\"");

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert - reading it twice is no different from reading it once
        result.Should().BeNull();
    }

    [Test]
    public static async Task ParseWebhook_UnrecognisedEnvironmentTag_ReturnsEvent()
    {
        // Arrange
        var json = CreateJson(tags: $"""["{Prefix}Staging"]""");

        // Act
        var result = await CreateParser().ParseWebhook(json);

        // Assert
        result.Should().NotBeNull();
    }

    private static string CreateJson(string? tags = null, string? tag = null)
    {
        var properties = new List<string>
        {
            $"\"event\":\"{EventName}\"",
            $"\"message-id\":\"{ExternalId}\""
        };

        if (tags != null)
        {
            properties.Add($"\"tags\":{tags}");
        }

        if (tag != null)
        {
            properties.Add($"\"tag\":{tag}");
        }

        return $"{{{string.Join(",", properties)}}}";
    }

    private static BrevoWebhookParser CreateParser(
        ILoggingService? loggingService = null,
        EnvironmentType environment = EnvironmentType.Dev,
        string prefix = Prefix)
        => new BrevoWebhookParser(
            loggingService ?? new Mock<ILoggingService>().Object,
            new BrevoWebhookParserSettings
            {
                Environment = environment,
                EnvironmentTagPrefix = prefix
            });
}
