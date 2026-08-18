using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Services.Members;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberLocaleServiceTests
{
    [Test]
    public static async Task GetCulture_NoStoredLocale_ReturnsDefault()
    {
        // Arrange - a member with no preferences at all.
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var service = new MemberLocaleService(MockUnitOfWorkFactory.Create(context));

        // Act
        var culture = await service.GetCulture(member.Id);

        // Assert
        culture.Should().Be(LocaleUtils.DefaultCulture);
    }

    [Test]
    public static async Task GetCulture_StoredLocale_ReturnsThatCulture()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        context.Create(new MemberPreferences { MemberId = member.Id, Locale = "fr-FR" });
        var service = new MemberLocaleService(MockUnitOfWorkFactory.Create(context));

        // Act
        var culture = await service.GetCulture(member.Id);

        // Assert
        culture.Name.Should().Be("fr-FR");
    }

    [Test]
    public static async Task GetCultures_MixOfStoredAndMissing_ResolvesEachWithDefaultFallback()
    {
        // Arrange - one member with a stored locale, one without.
        using var context = new MockOdkContext();
        var withLocale = context.CreateMember();
        var withoutLocale = context.CreateMember();
        context.Create(new MemberPreferences { MemberId = withLocale.Id, Locale = "en-US" });
        var service = new MemberLocaleService(MockUnitOfWorkFactory.Create(context));

        // Act
        var cultures = await service.GetCultures([withLocale.Id, withoutLocale.Id]);

        // Assert
        cultures[withLocale.Id].Name.Should().Be("en-US");
        cultures[withoutLocale.Id].Should().Be(LocaleUtils.DefaultCulture);
    }

    [Test]
    public static async Task UpdateLocale_DifferentLocale_PersistsIt()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        context.Create(new MemberPreferences { MemberId = member.Id, Locale = "en-GB" });
        var service = new MemberLocaleService(MockUnitOfWorkFactory.Create(context));

        // Act
        await service.UpdateLocale(member.Id, "en-US");

        // Assert
        context.Set<MemberPreferences>().Single(x => x.MemberId == member.Id).Locale.Should().Be("en-US");
    }

    [Test]
    public static async Task UpdateLocale_NoPreferences_CreatesThemWithTheLocale()
    {
        // Arrange - a member who has never had preferences (e.g. an admin-imported member).
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var service = new MemberLocaleService(MockUnitOfWorkFactory.Create(context));

        // Act
        await service.UpdateLocale(member.Id, "en-US");

        // Assert
        context.Set<MemberPreferences>().Single(x => x.MemberId == member.Id).Locale.Should().Be("en-US");
    }
}
