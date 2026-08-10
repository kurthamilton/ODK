using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ODK.Services.Emails;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailParametersTests
{
    [Test]
    public static void MirrorPrefix_MirrorsUnderTheTranslatedKey()
    {
        // Arrange - the exact key matters: a template references it by name, so "group..name" or
        // "groupname" would silently render as literal text in a sent email.
        var parameters = new Dictionary<string, string> { ["chapter.name"] = "Bristol" };

        // Act
        EmailParameters.MirrorPrefix(parameters, "chapter", "group");

        // Assert
        parameters.Should().ContainKey("group.name").WhoseValue.Should().Be("Bristol");
    }

    [Test]
    public static void MirrorPrefix_SeveralMatchingKeys_MirrorsAllOfThem()
    {
        // Arrange - more than one match, because mirroring writes into the dictionary it is reading and
        // a single-key case would not notice an enumerator being invalidated.
        var parameters = new Dictionary<string, string>
        {
            ["chapter.baseurl"] = "https://example.com/bristol",
            ["chapter.fullName"] = "Bristol Drunken Knitwits",
            ["chapter.name"] = "Bristol"
        };

        // Act
        var act = () => EmailParameters.MirrorPrefix(parameters, "chapter", "group");

        // Assert
        act.Should().NotThrow();
        parameters["group.name"].Should().Be("Bristol");
        parameters["group.fullName"].Should().Be("Bristol Drunken Knitwits");
        parameters["group.baseurl"].Should().Be("https://example.com/bristol");
    }

    [Test]
    public static void MirrorPrefix_LeavesTheOriginalKeysInPlace()
    {
        // Arrange - templates still using the old name have to keep working; this is a copy, not a rename.
        var parameters = new Dictionary<string, string> { ["chapter.name"] = "Bristol" };

        // Act
        EmailParameters.MirrorPrefix(parameters, "chapter", "group");

        // Assert
        parameters["chapter.name"].Should().Be("Bristol");
    }

    [Test]
    public static void MirrorPrefix_TargetAlreadySupplied_DoesNotOverwriteIt()
    {
        // Arrange - a caller that has moved to group.* should win over the mirrored legacy value.
        var parameters = new Dictionary<string, string>
        {
            ["chapter.name"] = "Legacy",
            ["group.name"] = "Supplied"
        };

        // Act
        EmailParameters.MirrorPrefix(parameters, "chapter", "group");

        // Assert
        parameters["group.name"].Should().Be("Supplied");
    }

    [Test]
    public static void MirrorPrefix_NonMatchingKeys_AreLeftAlone()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            ["chapter.name"] = "Bristol",
            ["platform.baseurl"] = "https://example.com",
            ["title"] = "Hello"
        };

        // Act
        EmailParameters.MirrorPrefix(parameters, "chapter", "group");

        // Assert
        parameters.Should().NotContainKey("group.baseurl");
        parameters.Should().NotContainKey("group.title");
        parameters.Count.Should().Be(4);
    }

    [Test]
    public static void MirrorPrefix_KeyMatchingThePrefixWithoutASeparator_IsNotMirrored()
    {
        // Arrange - "chapters" starts with "chapter" but is not a chapter.* parameter, so translating it
        // would invent a key nothing asked for.
        var parameters = new Dictionary<string, string> { ["chapters"] = "3" };

        // Act
        EmailParameters.MirrorPrefix(parameters, "chapter", "group");

        // Assert
        parameters.Should().ContainSingle();
    }

    [Test]
    public static void MirrorPrefix_NothingToMirror_DoesNothing()
    {
        // Arrange - a chapterless email has no chapter.* parameters at all.
        var parameters = new Dictionary<string, string> { ["platform.baseurl"] = "https://example.com" };

        // Act
        EmailParameters.MirrorPrefix(parameters, "chapter", "group");

        // Assert
        parameters.Should().ContainSingle();
    }
}
