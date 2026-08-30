using FluentAssertions;
using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Services.Emails;
using ODK.Services.Integrations.Emails.Brevo;
using ODK.Services.Integrations.Emails.Extensions;

namespace ODK.Services.Integrations.Tests.Emails.Extensions;

[Parallelizable]
public static class EmailClientEmailExtensionsTests
{
    private const string Prefix = "tag-prefix-";

    [Test]
    public static void ToBrevoRequest_EnvironmentNotSet_OmitsTags()
    {
        // Act
        var result = CreateEmail().ToBrevoRequest(CreateSettings(environment: EnvironmentType.None));

        // Assert
        result.Tags.Should().BeNull();
    }

    [Test]
    public static void ToBrevoRequest_EnvironmentSet_TagsWithEnvironment()
    {
        // Act
        var result = CreateEmail().ToBrevoRequest(CreateSettings());

        // Assert
        result.Tags.Should().BeEquivalentTo(["tag-prefix-Dev"]);
    }

    [Test]
    public static void ToBrevoRequest_PrefixNotSet_OmitsTags()
    {
        // Act
        var result = CreateEmail().ToBrevoRequest(CreateSettings(prefix: string.Empty));

        // Assert
        result.Tags.Should().BeNull();
    }

    private static EmailClientEmail CreateEmail() => new EmailClientEmail
    {
        BodyHtml = "<p>body</p>",
        From = new EmailAddressee("from@example.com", "From"),
        ScheduledUtc = null,
        Subject = "Subject",
        To = [new EmailAddressee("to@example.com", "To")]
    };

    private static BrevoApiEmailClientSettings CreateSettings(
        EnvironmentType environment = EnvironmentType.Dev,
        string prefix = Prefix)
        => new BrevoApiEmailClientSettings
        {
            ApiKey = "api-key",
            DebugEmailAddress = null,
            Environment = environment,
            EnvironmentTagPrefix = prefix
        };
}
