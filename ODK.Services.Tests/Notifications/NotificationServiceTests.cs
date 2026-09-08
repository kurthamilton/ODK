using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Notifications;

[Parallelizable]
public static class NotificationServiceTests
{
    [Test]
    public static async Task AddSubscriptionRenewedNotification_GroupOnDrunkenKnitwits_NamesItInFull()
    {
        /* Arrange - the text is persisted and read back with no request to say which platform it is being
           read on, so a Drunken Knitwits group has to be named unambiguously: "Bristol" alone does not say
           which Bristol, and the group is reachable from Group Squirrel too. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(
            name: "Bristol",
            platform: PlatformType.DrunkenKnitwits);
        var payment = context.CreatePayment(member: member, chapter: chapter);

        var service = CreateService(context);

        // Act
        await service.AddSubscriptionRenewedNotification(
            member, chapter, payment, nextPaymentUtc: null, settings: []);

        // The service adds and its caller commits, as the renewal path does, so the test commits for it.
        context.SaveChanges();

        // Assert
        var notification = context.Set<Notification>()
            .Single(x => x.MemberId == member.Id);
        notification.Text.Should().Be("Your membership of Bristol Drunken Knitwits has been renewed.");
    }

    private static MockOdkContext CreateMockOdkContext() => new();

    private static NotificationService CreateService(MockOdkContext context) => new(
        MockUnitOfWorkFactory.Create(context),
        Mock.Of<IMemberLocaleService>(x =>
            x.GetCulture(It.IsAny<Guid>()) == Task.FromResult(CultureInfo.InvariantCulture)));
}
