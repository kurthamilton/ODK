using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Emails.Parameters;
using ODK.Services.Web;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class TestEmailParametersFactoryTests
{
    [Test]
    public static async Task Create_EventInvite_NamesTheEventAndTheVenue()
    {
        /* Arrange - built from bare entities these came through empty, which reads as a broken template
           rather than as a stand-in: the preview showed the template's own tokens where the event's name and
           location belong. */
        var factory = new TestEmailParametersFactory(CreateUrlProviderFactory());

        // Act
        var result = await factory.Create(
            CreateRequest(),
            EmailType.EventInvite,
            CreateMember(),
            CultureInfo.InvariantCulture,
            CreateChapter());

        // Assert
        var parameters = result.ToDictionary();
        parameters["event.name"].Should().Be("EVENT NAME");
        parameters["event.location"].Should().Be("VENUE NAME");

        // A real date, so the stand-in event is not dated to DateTime.MinValue.
        parameters["event.date"].Should().NotContain("0001");
    }

    [TestCaseSource(nameof(EmailTypes))]
    public static async Task Create_EveryTypeExceptTheLayout_SuppliesSomeOfItsOwnParameters(EmailType type)
    {
        /* Arrange - a type with no entry in the factory sends a test email with its own tokens showing as
           literal braces. Nothing else refers to the factory, so adding an email type and leaving the
           factory alone is easy to do and invisible until someone sends a test. */
        var factory = new TestEmailParametersFactory(CreateUrlProviderFactory());

        // Act
        var result = await factory.Create(
            CreateRequest(), type, CreateMember(), CultureInfo.InvariantCulture, CreateChapter());

        /* Assert - some rather than all of what the type declares: a few parameters need a subject the
           factory has no stand-in for, and are deliberately left for the token to show through. */
        result.ToDictionary().Keys
            .Should().IntersectWith(EmailTemplateParameters.ForType(type));
    }

    /* The layout is excluded because it has no parameters of its own - it is the document every other email
       renders into, and takes its values from the email it wraps. */
    private static IEnumerable<EmailType> EmailTypes() => Enum.GetValues<EmailType>()
        .Where(x => x != EmailType.None && x != EmailType.Layout);

    private static Chapter CreateChapter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test group",
        Slug = "test-group",
        TimeZone = TimeZoneInfo.Utc
    };

    private static Member CreateMember() => new()
    {
        EmailAddress = "member@example.com",
        FirstName = "Test",
        Id = Guid.NewGuid(),
        LastName = "Member",
        TimeZone = TimeZoneInfo.Utc
    };

    private static IServiceRequest CreateRequest()
    {
        var mock = new Mock<IServiceRequest>();
        mock.Setup(x => x.HttpRequestContext).Returns(Mock.Of<IHttpRequestContext>());
        mock.Setup(x => x.Platform).Returns(PlatformType.Default);
        return mock.Object;
    }

    private static IUrlProviderFactory CreateUrlProviderFactory()
    {
        /* Every url-returning member answers, because the factory skips a null and this test would then
           read a missing parameter as a missing factory entry. */
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider.SetReturnsDefault("https://test.local/somewhere");

        var mock = new Mock<IUrlProviderFactory>();
        mock.Setup(x => x.Create(It.IsAny<IServiceRequest>())).ReturnsAsync(urlProvider.Object);
        return mock.Object;
    }
}
