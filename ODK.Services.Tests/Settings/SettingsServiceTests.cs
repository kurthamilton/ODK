using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Settings;
using ODK.Services.Settings.Models;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Settings;

[Parallelizable]
public static class SettingsServiceTests
{
    [Test]
    public static async Task ActivatePaymentSettings_DisabledSettings_Fails()
    {
        // Arrange - nothing can be bought through disabled settings, so activating them would leave the
        // platform unable to take a payment at all.
        using var context = CreateMockOdkContext();
        var active = context.CreateSitePaymentSettings();
        var disabled = context.CreateSitePaymentSettings(afterCreate: x =>
        {
            x.Active = false;
            x.Enabled = false;
        });
        var service = CreateService(context);

        // Act
        var result = await service.ActivatePaymentSettings(SiteAdminRequest(context), disabled.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SitePaymentSettings>().Single(x => x.Id == disabled.Id).Active.Should().BeFalse();
        context.Set<SitePaymentSettings>().Single(x => x.Id == active.Id).Active.Should().BeTrue();
    }

    [Test]
    public static async Task ActivatePaymentSettings_EnabledSettings_ActivatesThemAndDeactivatesTheRest()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var existing = context.CreateSitePaymentSettings();
        var activating = context.CreateSitePaymentSettings(afterCreate: x => x.Active = false);
        var service = CreateService(context);

        // Act
        var result = await service.ActivatePaymentSettings(SiteAdminRequest(context), activating.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SitePaymentSettings>().Single(x => x.Id == activating.Id).Active.Should().BeTrue();
        context.Set<SitePaymentSettings>().Single(x => x.Id == existing.Id).Active.Should().BeFalse();
    }

    [Test]
    public static async Task ActivatePaymentSettings_IdFromAnotherPlatform_Fails()
    {
        // Arrange - a platform only activates among its own settings, so another platform's row is not an
        // id it can name at all, however real that id is.
        using var context = CreateMockOdkContext();
        var otherPlatform = context.CreateSitePaymentSettings(
            PlatformType.DrunkenKnitwits, afterCreate: x => x.Active = false);
        var active = context.CreateSitePaymentSettings();
        var service = CreateService(context);

        // Act
        var result = await service.ActivatePaymentSettings(SiteAdminRequest(context), otherPlatform.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SitePaymentSettings>().Single(x => x.Id == otherPlatform.Id).Active.Should().BeFalse();
        context.Set<SitePaymentSettings>().Single(x => x.Id == active.Id).Active.Should().BeTrue();
    }

    [Test]
    public static async Task ActivatePaymentSettings_OtherPlatformsSettings_LeavesThemActive()
    {
        // Arrange - each platform transacts through its own row, so one platform's active row is not the
        // other's to switch off.
        using var context = CreateMockOdkContext();
        var otherPlatform = context.CreateSitePaymentSettings(PlatformType.DrunkenKnitwits);
        var activating = context.CreateSitePaymentSettings(afterCreate: x => x.Active = false);
        var service = CreateService(context);

        // Act
        var result = await service.ActivatePaymentSettings(SiteAdminRequest(context), activating.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SitePaymentSettings>().Single(x => x.Id == otherPlatform.Id).Active.Should().BeTrue();
    }

    [Test]
    public static async Task CreatePaymentSettings_NewSettings_RecordsTheRequestsPlatform()
    {
        /* Arrange - the platform is what scopes every later read of these settings, so a row created without
           one is invisible to the platform that created it. Arranged on Drunken Knitwits so the assertion
           cannot pass on the enum's own zero. */
        using var context = CreateMockOdkContext();
        var service = CreateService(context);

        // Act
        var result = await service.CreatePaymentSettings(
            SiteAdminRequest(context, PlatformType.DrunkenKnitwits),
            new SitePaymentSettingsCreateModel
            {
                ApiPublicKey = "pk",
                ApiSecretKey = "sk",
                Commission = 0.05m,
                Enabled = true,
                ExternalId = "acct_test",
                ExternalUrl = "https://dashboard.stripe.com/acct_test",
                Name = "New",
                Provider = PaymentProviderType.Stripe
            });

        // Assert
        result.Success.Should().BeTrue();

        var created = context.Set<SitePaymentSettings>().Single();
        created.Platform.Should().Be(PlatformType.DrunkenKnitwits);
    }

    [Test]
    public static async Task UpdatePaymentSettings_ActiveSettingsBeingDisabled_Fails()
    {
        // Arrange - the platform transacts through its active row, so disabling it would stop payments.
        using var context = CreateMockOdkContext();
        var settings = context.CreateSitePaymentSettings();
        var service = CreateService(context);

        // Act
        var result = await service.UpdatePaymentSettings(
            SiteAdminRequest(context),
            settings.Id,
            new SitePaymentSettingsUpdateModel
            {
                ApiPublicKey = "pk",
                ApiSecretKey = "sk",
                Commission = 0.05m,
                Enabled = false,
                ExternalId = "acct_test",
                ExternalUrl = "https://dashboard.stripe.com/acct_test",
                Name = "Updated"
            });

        // Assert
        result.Success.Should().BeFalse();

        var updated = context.Set<SitePaymentSettings>().Single(x => x.Id == settings.Id);
        updated.Enabled.Should().BeTrue();
        updated.Name.Should().NotBe("Updated");
    }

    [Test]
    public static async Task UpdatePaymentSettings_InactiveSettingsBeingDisabled_Disables()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var settings = context.CreateSitePaymentSettings(afterCreate: x => x.Active = false);
        var service = CreateService(context);

        // Act
        var result = await service.UpdatePaymentSettings(
            SiteAdminRequest(context),
            settings.Id,
            new SitePaymentSettingsUpdateModel
            {
                ApiPublicKey = "pk",
                ApiSecretKey = "sk",
                Commission = 0.05m,
                Enabled = false,
                ExternalId = "acct_test",
                ExternalUrl = "https://dashboard.stripe.com/acct_test",
                Name = "Updated"
            });

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SitePaymentSettings>().Single(x => x.Id == settings.Id).Enabled.Should().BeFalse();
    }

    private static MockOdkContext CreateMockOdkContext() => new();

    private static SettingsService CreateService(MockOdkContext context) =>
        new(MockUnitOfWorkFactory.Create(context));

    private static IMemberServiceRequest SiteAdminRequest(
        MockOdkContext context, PlatformType platform = PlatformType.Default)
    {
        var siteAdmin = context.CreateMember(afterCreate: x => x.SiteAdmin = true);

        return Mock.Of<IMemberServiceRequest>(x =>
            x.Platform == platform &&
            x.CurrentMember == siteAdmin);
    }
}
