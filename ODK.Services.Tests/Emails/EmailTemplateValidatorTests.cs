using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Emails;
using ODK.Services.Emails.Parameters;
using ODK.Services.Emails.Validation;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailTemplateValidatorTests
{
    private static readonly string[] Supplied = ["group.name", "member.firstName", "html:member.properties"];

    [Test]
    public static void UnknownPlaceholders_AllKnown_ReturnsNone()
    {
        // Act
        var result = EmailTemplateValidator.UnknownPlaceholders(
            "<p>{group.name} {member.firstName}</p>", Supplied);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public static void UnknownPlaceholders_MisspeltName_ReturnsIt()
    {
        // Arrange - the case this exists for. Interpolation leaves an unrecognised token exactly as
        // written, so without this it reaches the member as literal braces.
        // Act
        var result = EmailTemplateValidator.UnknownPlaceholders("<p>{group.nmae}</p>", Supplied);

        // Assert
        result.Should().Equal("group.nmae");
    }

    [Test]
    public static void UnknownPlaceholders_DifferentCasing_IsNotReported()
    {
        // Arrange - placeholders resolve case-insensitively, so {Group.Name} renders correctly and must
        // not be rejected.
        // Act
        var result = EmailTemplateValidator.UnknownPlaceholders("<p>{Group.Name}</p>", Supplied);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public static void UnknownPlaceholders_Css_IsNotReported()
    {
        // Arrange - the layout template carries a stylesheet. A declaration block matches the pattern
        // interpolation uses, so a looser check would report one on every save of the layout.
        var body = "<style>body { color: red; }</style><p>{group.name}</p>";

        // Act
        var result = EmailTemplateValidator.UnknownPlaceholders(body, Supplied);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public static void UnknownPlaceholders_HtmlPrefixedName_IsNotReported()
    {
        // Arrange - a template refers to a pre-encoded value by its plain name, but it is supplied under
        // the prefix, so both spellings have to be accepted.
        // Act
        var result = EmailTemplateValidator.UnknownPlaceholders(
            "<p>{html:member.properties}</p>", Supplied);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public static void UnknownPlaceholders_SameNameTwice_ReturnsItOnce()
    {
        // Act
        var result = EmailTemplateValidator.UnknownPlaceholders("{a.b} and {a.b}", Supplied);

        // Assert
        result.Should().Equal("a.b");
    }

    [TestCaseSource(nameof(EmailTypes))]
    public static void UnknownPlaceholders_EveryOfferedPlaceholder_IsAccepted(EmailType type)
    {
        // Arrange - the editor offers these as buttons, so inserting one and saving must not then be
        // rejected by the very validation the same page applies.
        var supplied = EmailTemplateParameters.ForSite(type);
        var template = string.Join(" ", EmailTemplateParameters.ForGroup(type).Select(x => $"{{{x}}}"));

        // Act
        var result = EmailTemplateValidator.UnknownPlaceholders(template, supplied);

        // Assert
        result.Should().BeEmpty();
    }

    private static System.Collections.Generic.IEnumerable<EmailType> EmailTypes()
        => System.Enum.GetValues<EmailType>().Where(x => x != EmailType.None);
}
