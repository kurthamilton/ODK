using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// A site subscription that has run out keeps what it paid for until a cooldown period is up, so what
/// decides whether an owner still has their features is not the stored expiry date but which side of that
/// window it falls on. These cover both places the app asks the question - the feature gate a group's admin
/// pages are rendered from, and the check that lets an owner create another group - by lapsing a
/// subscription just inside the window and just beyond it. Only arranging both sides distinguishes the
/// cooldown being applied from it being ignored: a lapsed subscription is stored the same way either way.
/// <para>
/// The window's length is the app's own configuration, which these tests cannot read, so it is stated again
/// as <see cref="E2ESettings.SiteSubscriptionCooldownMonths"/> and the two have to agree. Where that says
/// the app runs with no cooldown the fixture skips rather than fails: no cooldown is a valid way to run the
/// app, and it leaves nothing here to assert - the without-the-feature cases other fixtures already cover
/// are what a lapsed subscription then means.
/// </para>
/// </summary>
[TestFixture]
public class SiteSubscriptionCooldownTests : DefaultPageTest
{
    // What a refused create comes back with (ErrorMessagesResource.SubscriptionExpired). Named because it is
    // authored wording that may change.
    private const string SubscriptionExpiredFeedback = "Your subscription has expired";

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    [SetUp]
    public void RequireACooldown()
    {
        if (E2ESettings.SiteSubscriptionCooldownMonths <= 0)
        {
            Assert.Ignore(
                "The app under test is configured with no site subscription cooldown " +
                "(Subscriptions:DefaultCooldownMonths), so there is no window to be inside.");
        }
    }

    // Lapsed yesterday - inside the cooldown however short it is, so the feature is still the group's.
    [TestCase(true)]
    // Lapsed the day before the window opened, so it is out of reach however long the window is.
    [TestCase(false)]
    public async Task ChapterEmail_OwnerSubscriptionLapsed_IsCustomisableOnlyWithinTheCooldown(
        bool withinCooldown)
    {
        // Arrange - a group on a subscription covering custom emails, with wording of its own already
        // written. Customising first is what makes the locked field mean the feature has gone: a field left
        // on the site's default is locked whatever the subscription says.
        var subscription = await Provisioning.EnsureCustomEmailsSiteSubscription();

        var owner = await Provisioning.NewAccount("cooldown-email-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2ecooldown{Guid.NewGuid():N}");

        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.EnsureActive(ownerId, subscription.Id, subscription.PriceId);

        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        var page = new ChapterEmailAdminPage(Page);
        var emailUrl = await page.OpenFirstEmail(PlatformRoutes.Default(group).EmailsAdmin);
        await page.SetCustomWording(emailUrl, subject: $"E2E cooldown {Guid.NewGuid():N}");

        // Act - the subscription runs out, on one side of the cooldown or the other.
        await MemberSubscriptions.Expire(ownerId, LapsedAt(withinCooldown));
        await page.Open(emailUrl);

        // Assert - the wording can still be written while the cooldown covers the group, and not once it
        // does not.
        (await page.IsSubjectEditable()).Should().Be(withinCooldown);
    }

    [Test]
    public async Task CreateGroup_OwnerSubscriptionLapsedWithinTheCooldown_CreatesTheGroup()
    {
        // Arrange - an owner whose paid subscription ran out yesterday.
        var owner = await LapsedSubscriber(withinCooldown: true);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act
        var chapterId = await new CreateGroupPage(Page).CreateGroup(NewGroupName());

        // Assert - the wizard finished and the group is there, which is the cooldown still standing in for a
        // live subscription.
        (await ChapterDataHelper.GetSlug(chapterId)).Should().NotBeEmpty();
    }

    [Test]
    public async Task CreateGroup_OwnerSubscriptionLapsedBeyondTheCooldown_IsRefused()
    {
        // Arrange - the same owner, lapsed a day further back than the cooldown reaches.
        var owner = await LapsedSubscriber(withinCooldown: false);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act
        var feedback = await new CreateGroupPage(Page).CreateGroupExpectingRefusal(NewGroupName());

        // Assert - refused for the subscription rather than for anything else the form could object to. The
        // owner has no group, so the group limit cannot be what stopped it.
        feedback.Should().Contain(SubscriptionExpiredFeedback);
    }

    /// <summary>
    /// An expiry just inside the cooldown, or just beyond it. Inside is yesterday, which a cooldown of any
    /// length covers; beyond is a day before the window opened. Both are a day clear of the boundary, so the
    /// app resolving the window a moment later than this cannot land on the wrong side of it.
    /// </summary>
    private static DateTime LapsedAt(bool withinCooldown)
        => withinCooldown
            ? DateTime.UtcNow.AddDays(-1)
            : DateTime.UtcNow.AddMonths(-E2ESettings.SiteSubscriptionCooldownMonths).AddDays(-1);

    /// <summary>
    /// An account on a paid site subscription that has since run out, on the given side of the cooldown. It
    /// owns no group, so the group limit is never what a refused create is about. Which subscription it is
    /// does not matter here - creating a group turns on the expiry, not on any feature - so it reuses the one
    /// this fixture already provisions rather than adding another to the run.
    /// </summary>
    private static async Task<TestAccount> LapsedSubscriber(bool withinCooldown)
    {
        var subscription = await Provisioning.EnsureCustomEmailsSiteSubscription();

        var owner = await Provisioning.NewAccount("cooldown-group-owner");
        var ownerId = await Members.GetMemberId(owner.Email);

        await MemberSubscriptions.EnsureActive(ownerId, subscription.Id, subscription.PriceId);
        await MemberSubscriptions.Expire(ownerId, LapsedAt(withinCooldown));

        return owner;
    }

    private static string NewGroupName() => $"E2E cooldown {Guid.NewGuid():N}";
}
