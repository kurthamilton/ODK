using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Services.Countries;
using ODK.Services.Exceptions;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Countries;

[Parallelizable]
public static class CountryAdminServiceTests
{
    [Test]
    public static async Task GetCountries_AsNonSiteAdmin_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.CreateCountry(isoCode2: "US");
        var member = context.CreateMember(siteAdmin: false);
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act / Assert
        await FluentActions.Awaiting(() => service.GetCountries(CreateRequest(member)))
            .Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task GetCountries_AsSiteAdmin_ReturnsAllCountries()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.CreateCountry(isoCode2: "US");
        context.CreateCountry(isoCode2: "GB");
        var member = context.CreateMember(siteAdmin: true);
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act
        var countries = await service.GetCountries(CreateRequest(member));

        // Assert
        countries.Should().HaveCount(2);
    }

    private static IMemberServiceRequest CreateRequest(Member member)
    {
        var mock = new Mock<IMemberServiceRequest>();
        mock.Setup(x => x.CurrentMember).Returns(member);
        mock.Setup(x => x.CurrentMemberOrDefault).Returns(member);
        return mock.Object;
    }
}
