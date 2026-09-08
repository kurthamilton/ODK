using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// What becomes of a site subscription once it has lapsed for good. The stored expiry is never moved, so
/// nothing happens at the moment one runs out - a sweep
/// (<c>POST /scheduledtasks/subscriptions/lapsed/downgrade</c>, run by an external cron) moves the member
/// onto the platform's default free plan once the cooldown on top of the expiry has passed too. Nothing in
/// the UI runs it, so these drive the endpoint and then look at what the member is left holding.
/// <para>
/// Both sides of the cooldown are arranged, for the reason <see cref="SiteSubscriptionCooldownTests"/>
/// gives: a lapsed subscription is stored the same way either way, so only covering both distinguishes the
/// sweep honouring the window from it moving everybody.
/// </para>
/// </summary>
[TestFixture]
/* The sweep is platform-wide - it downgrades every member the platform has lapsed, not only this fixture's
   - so it must not run while another fixture has one arranged. SiteSubscriptionCooldownTests lapses
   subscriptions beyond the cooldown and asserts a group create is refused for the expiry, which a downgrade
   underneath it would answer with the free plan's group limit instead. */
[NonParallelizable]
public class SiteSubscriptionDowngradeTests : DefaultPageTest
{
    // What the downgrade email's subject carries (MemberEmailService.SendSiteSubscriptionExpiredEmail, whose
    // subject is "{title} - Subscription Expired"). Named because it is authored wording that may change.
    private const string SubscriptionExpiredEmailSubject = "Subscription Expired";

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    private static NotificationDataHelper Notifications => new(E2ESettings.ConnectionString);

    private static SiteSubscriptionDataHelper Subscriptions => new(E2ESettings.ConnectionString);

    [Test]
    public async Task DowngradeLapsedSiteSubscriptions_LapsedBeyondTheCooldown_MovesTheMemberOntoTheFreePlan()
    {
        // Arrange - a member on a paid plan that ran out a day further back than the cooldown reaches. Where
        // they land is the platform's own default plan, which is where every account starts.
        var freePlanName = await Subscriptions.GetDefaultFreeName(PlatformTypeId, E2ESettings.EnvironmentTypeId)
            ?? throw new InvalidOperationException(
                "The app under test has no enabled, default, free site subscription for this platform, which " +
                "is what a lapsed member is moved onto - so there is nothing for the sweep to do.");

        var member = await LapsedSubscriber(withinCooldown: false);
        var memberId = await Members.GetMemberId(member.Email);

        // Act
        await ScheduledTasks.DowngradeLapsedSiteSubscriptions(Page, PlatformBaseUrl);

        // Assert - the member's own page says which plan they are on, and it is now the free one.
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        var accountPage = new SiteSubscriptionAccountPage(Page);
        await accountPage.GoTo();
        if (await accountPage.IsCurrencyPromptShown())
        {
            await accountPage.ChooseFirstCurrency();
        }

        (await accountPage.GetCurrentSubscriptionName()).Should().Be(freePlanName);

        // A free plan never expires, which is also what stops the sweep reaching this member again.
        (await MemberSubscriptions.GetExpiresUtc(memberId)).Should().BeNull();

        // The paid record is kept rather than rewritten, so the downgrade is a second row: the log is the
        // history a reinstatement is read back from.
        (await MemberSubscriptions.GetRecordCount(memberId)).Should().Be(2);

        // Nothing the member did prompted this, so they are told - in the app, and by email, since somebody
        // who has stopped paying is not necessarily visiting the site.
        var notifications = await Notifications.GetByType(memberId, NotificationTypeIds.SubscriptionDowngraded);
        notifications.Should().ContainSingle();
        notifications.Single().ChapterId.Should().BeNull("a site subscription belongs to no group");
        notifications.Single().Text.Should().Contain(freePlanName, "the member is told which plan they are on now");

        (await SentEmailDataHelper.GetSubjectsContaining(
            member.Email, SubscriptionExpiredEmailSubject, expectedCount: 1))
            .Should().ContainSingle();
    }

    [Test]
    public async Task DowngradeLapsedSiteSubscriptions_LapsedWithinTheCooldown_LeavesTheSubscriptionAlone()
    {
        SiteSubscriptionCooldownWindow.Require();

        // Arrange - the same member, whose subscription ran out only yesterday.
        var member = await LapsedSubscriber(withinCooldown: true);
        var memberId = await Members.GetMemberId(member.Email);
        var expiresUtc = await MemberSubscriptions.GetExpiresUtc(memberId);

        // Act
        await ScheduledTasks.DowngradeLapsedSiteSubscriptions(Page, PlatformBaseUrl);

        // Assert - untouched. An expired subscription keeps what it paid for until the cooldown is up, so
        // there is nothing yet to move the member off or to tell them about.
        (await MemberSubscriptions.GetRecordCount(memberId)).Should().Be(1);
        (await MemberSubscriptions.GetExpiresUtc(memberId)).Should().Be(expiresUtc);
        (await Notifications.GetByType(memberId, NotificationTypeIds.SubscriptionDowngraded))
            .Should().BeEmpty();
    }

    /// <summary>
    /// An account on a paid site subscription that has since run out, on the given side of the cooldown.
    /// Which subscription it is does not matter - the sweep turns on the expiry, not on any feature - so it
    /// reuses one the run already provisions rather than adding another to it.
    /// </summary>
    private static async Task<TestAccount> LapsedSubscriber(bool withinCooldown)
    {
        var subscription = await Provisioning.EnsureCustomEmailsSiteSubscription();

        var member = await Provisioning.NewAccount("downgrade-subscriber");
        var memberId = await Members.GetMemberId(member.Email);

        // Upgrades the free record account creation left, so the member still has exactly one - which is
        // what makes the second row the downgrade wrote countable.
        await MemberSubscriptions.EnsureActive(memberId, subscription.Id, subscription.PriceId);
        await MemberSubscriptions.Expire(memberId, SiteSubscriptionCooldownWindow.LapsedAt(withinCooldown));

        return member;
    }
}
