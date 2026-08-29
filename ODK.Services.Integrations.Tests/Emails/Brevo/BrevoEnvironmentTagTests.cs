using FluentAssertions;
using ODK.Core.Platforms;
using ODK.Services.Integrations.Emails.Brevo;

namespace ODK.Services.Integrations.Tests.Emails.Brevo;

[Parallelizable]
public static class BrevoEnvironmentTagTests
{
    private const string Prefix = "tag-prefix-";

    [Test]
    public static void Format_EmptyPrefix_ReturnsNull()
    {
        // Act
        var result = BrevoEnvironmentTag.Format(string.Empty, EnvironmentType.Dev);

        // Assert - an unconfigured prefix is no format to state a tag in
        result.Should().BeNull();
    }

    [Test]
    public static void Format_NoEnvironment_ReturnsNull()
    {
        // Act
        var result = BrevoEnvironmentTag.Format(Prefix, EnvironmentType.None);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void Format_PrefixAndEnvironment_ReturnsTag()
    {
        // Act
        var result = BrevoEnvironmentTag.Format(Prefix, EnvironmentType.Dev);

        // Assert
        result.Should().Be("tag-prefix-Dev");
    }

    [Test]
    public static void Parse_DifferentCasing_ReturnsEnvironment()
    {
        // Act
        var result = BrevoEnvironmentTag.Parse(Prefix, ["TAG-PREFIX-dev"]);

        // Assert
        result.Should().Be(EnvironmentType.Dev);
    }

    [Test]
    public static void Parse_EmptyPrefix_ReturnsNone()
    {
        // Act
        var result = BrevoEnvironmentTag.Parse(string.Empty, ["tag-prefix-Dev"]);

        // Assert - without a prefix there is nothing to recognise a tag by
        result.Should().Be(EnvironmentType.None);
    }

    [Test]
    public static void Parse_FormattedTag_ReturnsEnvironment()
    {
        // Arrange
        var tag = BrevoEnvironmentTag.Format(Prefix, EnvironmentType.Prod);

        // Act
        var result = BrevoEnvironmentTag.Parse(Prefix, [tag!]);

        // Assert
        result.Should().Be(EnvironmentType.Prod);
    }

    [Test]
    public static void Parse_NoneNamedInTag_ReturnsNone()
    {
        // Act
        var result = BrevoEnvironmentTag.Parse(Prefix, ["tag-prefix-None"]);

        // Assert
        result.Should().Be(EnvironmentType.None);
    }

    [Test]
    public static void Parse_NoTags_ReturnsNone()
    {
        // Act
        var result = BrevoEnvironmentTag.Parse(Prefix, []);

        // Assert
        result.Should().Be(EnvironmentType.None);
    }

    [Test]
    public static void Parse_TagAmongOthers_ReturnsEnvironment()
    {
        // Act
        var result = BrevoEnvironmentTag.Parse(Prefix, ["welcome-email", "tag-prefix-E2E", "other"]);

        // Assert
        result.Should().Be(EnvironmentType.E2E);
    }

    [Test]
    public static void Parse_UnprefixedTag_ReturnsNone()
    {
        // Act
        var result = BrevoEnvironmentTag.Parse(Prefix, ["Dev"]);

        // Assert
        result.Should().Be(EnvironmentType.None);
    }

    [Test]
    public static void Parse_UnrecognisedEnvironment_ReturnsNone()
    {
        // Act
        var result = BrevoEnvironmentTag.Parse(Prefix, ["tag-prefix-Staging"]);

        // Assert - a tag naming nothing this app knows is no statement, not a mismatch
        result.Should().Be(EnvironmentType.None);
    }
}
