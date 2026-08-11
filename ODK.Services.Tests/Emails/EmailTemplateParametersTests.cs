using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Emails.Parameters;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailTemplateParametersTests
{
    [Test]
    public static void ForType_EveryEmailTypeIsRegistered()
    {
        // Arrange - a type left out of the registry throws when its template is opened, which is a 500
        // on an admin page. Every type except the None sentinel has a template someone can edit.
        var types = Enum.GetValues<EmailType>().Where(x => x != EmailType.None);

        // Act / Assert
        foreach (var type in types)
        {
            var act = () => EmailTemplateParameters.ForType(type);
            act.Should().NotThrow($"{type} should be registered");
        }
    }

    [Test]
    public static void ForType_UnregisteredType_Throws()
    {
        // Act
        var act = () => EmailTemplateParameters.ForType(EmailType.None);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*None*");
    }

    [TestCaseSource(nameof(ParameterTypes))]
    public static void Names_AreAllSuppliedByTheType(Type parametersType)
    {
        // Arrange - every property set, so AddParameters emits everything it can. What an admin is
        // offered has to be a subset of that: an offered name nothing supplies renders as literal
        // braces in a sent email. Supplying more than is offered is fine - that is the legacy spellings.
        var instance = (EmailTypeParameters)Activator.CreateInstance(parametersType)!;
        foreach (var property in Settable(parametersType))
        {
            property.SetValue(instance, "value");
        }

        var offered = Names(parametersType);

        // Act - the HTML prefix marks a value as pre-encoded; templates use the unprefixed name.
        var supplied = instance.ToDictionary().Keys
            .Select(x => x.StartsWith(EmailParameters.HtmlPrefix, StringComparison.Ordinal)
                ? x[EmailParameters.HtmlPrefix.Length..]
                : x)
            .ToArray();

        // Assert - excluding what the core supplies rather than the type: the body, which EmailService
        // sets after everything else is resolved, and the theme colours the layout re-offers.
        offered
            .Where(x => x != EmailParameters.BodyName && !EmailParameters.Names.Contains(x))
            .Should().BeSubsetOf(supplied);
    }

    [TestCaseSource(nameof(EmailTypes))]
    public static void ForGroupAndForSite_OfferEachPlaceholderOnce(EmailType type)
    {
        // Arrange - core and type names are concatenated, so an overlap would render a button twice.
        // Act
        var group = EmailTemplateParameters.ForGroup(type);
        var site = EmailTemplateParameters.ForSite(type);

        // Assert
        group.Should().OnlyHaveUniqueItems();
        site.Should().OnlyHaveUniqueItems();
    }

    [TestCaseSource(nameof(EmailTypes))]
    public static void ForSite_OffersTheThemeColoursOnlyOnTheLayout(EmailType type)
    {
        // Arrange - they style the layout's own markup, so anywhere else they are buttons that do
        // nothing useful. Still supplied everywhere; this is only about what is offered.
        // Act
        var offered = EmailTemplateParameters.ForSite(type);

        // Assert
        if (type == EmailType.Layout)
        {
            offered.Should().Contain(EmailParameters.ThemeNames);
        }
        else
        {
            offered.Should().NotIntersectWith(EmailParameters.ThemeNames);
        }
    }

    [TestCaseSource(nameof(EmailTypes))]
    public static void ForGroup_NeverOffersTheThemeColours(EmailType type)
    {
        // Arrange - the colours are the site's, not one group's, so a group is not offered them even on
        // the layout, which it can otherwise override.
        // Act
        var offered = EmailTemplateParameters.ForGroup(type);

        // Assert
        offered.Should().NotIntersectWith(EmailParameters.ThemeNames);
    }

    private static IEnumerable<EmailType> EmailTypes() => Enum.GetValues<EmailType>()
        .Where(x => x != EmailType.None);

    private static IEnumerable<Type> ParameterTypes() => typeof(EmailTypeParameters).Assembly
        .GetTypes()
        .Where(x => x.IsSealed && typeof(EmailTypeParameters).IsAssignableFrom(x))
        .OrderBy(x => x.Name);

    private static IReadOnlyCollection<string> Names(Type parametersType)
    {
        var property = parametersType.GetProperty("Names", BindingFlags.Public | BindingFlags.Static);
        property.Should().NotBeNull($"{parametersType.Name} should declare a static Names");

        return (IReadOnlyCollection<string>)property!.GetValue(null)!;
    }

    private static IEnumerable<PropertyInfo> Settable(Type parametersType) => parametersType
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(x => x.PropertyType == typeof(string) && x.CanWrite);
}
