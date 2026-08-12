using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Services.Emails;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailParametersTests
{
    [Test]
    public static void Names_IncludesTheTitle()
    {
        // Arrange - the title is the one parameter with no property, because EmailService resolves it
        // after merging. It is easily lost when the others are refactored.
        // Act / Assert
        EmailParameters.Names.Should().Contain(EmailParameters.TitleName);
    }

    [Test]
    public static void GroupNames_IsASubsetOfNames()
    {
        // Arrange - what a group admin is offered has to be a narrowing of what the app supplies, not a
        // separate list. Only then does the "every offered placeholder resolves" guarantee cover it too.
        // Act / Assert
        EmailParameters.GroupNames.Should().BeSubsetOf(EmailParameters.Names);
    }

    [Test]
    public static void GroupNames_IsTheGroupParametersAndTheTitle()
    {
        // Act
        var result = EmailParameters.GroupNames;

        // Assert - platform and theme values are the site's to set, so a group is not offered them.
        result.Should().Contain(EmailParameters.TitleName);
        result.Should().OnlyContain(x => x.StartsWith("group.") || x == EmailParameters.TitleName);
        result.Should().NotContain("platform.url");
    }

    [Test]
    public static void ToDictionary_PropertyNotSet_OmitsIt()
    {
        // Arrange - a chapterless email has no group url. Omitted rather than empty, so a template
        // referencing it shows the token instead of silently rendering a blank.
        var parameters = new EmailParameters { GroupName = "Bristol" };

        // Act
        var result = parameters.ToDictionary();

        // Assert
        result.Should().ContainKey("group.name");
        result.Should().NotContainKey("group.url");
    }

    [Test]
    public static void ToDictionary_EveryPropertySet_ProducesEveryNameExceptTheTitle()
    {
        // Arrange
        var parameters = new EmailParameters
        {
            GroupUrl = "https://example.com/bristol",
            GroupFullName = "Bristol Drunken Knitwits",
            GroupName = "Bristol",
            PlatformUrl = "https://example.com",
            ThemeBodyBackground = "#fff",
            ThemeBodyColor = "#000",
            ThemeHeaderBackground = "#eee",
            ThemeHeaderColor = "#111"
        };

        // Act
        var result = parameters.ToDictionary();

        // Assert - a property added without a matching name would go unnoticed otherwise: it would
        // simply never appear in a sent email, and never be offered to an admin.
        result.Keys.Should().BeEquivalentTo(
            EmailParameters.Names.Where(x => x != EmailParameters.TitleName));
    }
}
