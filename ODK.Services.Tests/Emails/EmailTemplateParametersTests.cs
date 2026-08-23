using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Emails;
using ODK.Services.Emails.Parameters;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailTemplateParametersTests
{
    private static readonly DateTime SampleDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

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
    public static void Descriptions_EveryOfferedParameterIsDescribed()
    {
        // Arrange - the properties table on both email admin pages is built from these, so a parameter with
        // no description is a blank row rather than a crash: nothing would surface it but this. ForSite is
        // the wider list, so covering it covers what a group is offered too.
        var types = Enum.GetValues<EmailType>().Where(x => x != EmailType.None);

        var undescribed = types
            .SelectMany(EmailTemplateParameters.ForSite)
            .Distinct(EmailParameterComparer.Default)
            .Where(x => EmailParameterDescriptions.For(x) == null)
            .ToArray();

        // Assert
        undescribed.Should().BeEmpty(
            $"every parameter needs a row in {nameof(EmailParameterDescriptions)}");
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
        var instance = Create(parametersType);
        foreach (var property in Settable(parametersType))
        {
            property.SetValue(instance, SampleValue(property.PropertyType));
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

    /* Types are still discovered by reflection so a new parameters class cannot slip past this test, but a
       class taking constructor arguments has to be built by hand. An unlisted one falls through to the
       parameterless path and fails with a message saying to add it, which is the same bargain
       EmailTemplateParameters makes with its own registry. */
    private static EmailTypeParameters Create(Type parametersType)
    {
        if (parametersType == typeof(EventCommentParameters))
        {
            return new EventCommentParameters(Event())
            {
                EventUrl = "value",
                Text = "value"
            };
        }

        if (parametersType == typeof(EventInviteParameters))
        {
            return new EventInviteParameters(Chapter(), Event(), Venue(), CultureInfo.InvariantCulture)
            {
                RsvpUrl = "value",
                UnsubscribeUrl = "value",
                Url = "value"
            };
        }

        if (parametersType == typeof(PaymentNotificationParameters))
        {
            return new PaymentNotificationParameters(Currency())
            {
                Amount = 1.23M,
                Reference = "value"
            };
        }

        if (parametersType == typeof(SubscriptionConfirmationParameters))
        {
            return new SubscriptionConfirmationParameters(Currency(), Member(), CultureInfo.InvariantCulture)
            {
                Amount = 1.23M,
                ExpiresUtc = SampleDate
            };
        }

        if (parametersType == typeof(SubscriptionExpiryParameters))
        {
            return new SubscriptionExpiryParameters(Member(), CultureInfo.InvariantCulture)
            {
                DisabledUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow
            };
        }

        var instance = Activator.CreateInstance(parametersType) as EmailTypeParameters;
        instance.Should().NotBeNull(
            $"{parametersType.Name} takes constructor arguments, so add it to {nameof(Create)}");

        return instance!;
    }

    private static Chapter Chapter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test group",
        Slug = "test-group",
        TimeZone = TimeZoneInfo.Utc
    };

    private static Currency Currency() => new()
    {
        Code = "GBP",
        Id = Guid.NewGuid(),
        Symbol = "£"
    };

    private static IEnumerable<EmailType> EmailTypes() => Enum.GetValues<EmailType>()
        .Where(x => x != EmailType.None);

    private static Event Event() => new()
    {
        DateUtc = SampleDate,
        Id = Guid.NewGuid(),
        Name = "Test event"
    };

    private static Member Member() => new()
    {
        EmailAddress = "member@example.com",
        FirstName = "Test",
        Id = Guid.NewGuid(),
        LastName = "Member",
        TimeZone = TimeZoneInfo.Utc
    };

    /* Every property, not only the strings: a parameter now holds its unformatted value and the class
       formats it, so leaving the non-strings unset would report the class as failing to supply a name it
       supplies perfectly well. */
    private static object SampleValue(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(string))
        {
            return "value";
        }

        if (type == typeof(decimal))
        {
            return 1.23M;
        }

        if (type == typeof(DateTime))
        {
            return SampleDate;
        }

        throw new NotSupportedException(
            $"No sample value for {type.Name}. Add one so parameters holding it stay covered.");
    }

    private static Venue Venue() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test venue"
    };

    private static IEnumerable<Type> ParameterTypes() => typeof(EmailTypeParameters).Assembly
        .GetTypes()
        .Where(x => x.IsSealed && typeof(EmailTypeParameters).IsAssignableFrom(x))
        .OrderBy(x => x.Name);

    private static IReadOnlyCollection<string> Names(Type parametersType)
    {
        var property = parametersType.GetProperty("Names", BindingFlags.Public | BindingFlags.Static);
        property.Should().NotBeNull($"{parametersType.Name} should declare a static Names");

        return (IReadOnlyCollection<string>)property.GetValue(null)!;
    }

    private static IEnumerable<PropertyInfo> Settable(Type parametersType) => parametersType
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(x => x.CanWrite);
}
