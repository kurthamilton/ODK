using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// The DrunkenKnitwits analog of <see cref="AccountFlowTests"/>. On DrunkenKnitwits there is no
/// self-service group creation and no global sign-up: joining a chapter is the sign-up. So the journey
/// is chapter-scoped - join (= create account) -> activate -> log in - against a seeded chapter.
/// </summary>
[TestFixture]
[Category("AccountWorkflows")]
[Category("ChapterMembershipWorkflows")]
public class DrunkenKnitwitsAccountFlowTests : DrunkenKnitwitsPageTest
{
    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    [Test]
    public async Task JoinChapter_ActivateAndLogIn_Succeeds()
    {
        // Arrange - a published DrunkenKnitwits chapter to join. DrunkenKnitwits has no self-service
        // creation, so it's created through the Default UI then re-platformed. The name is URL-safe
        // because the DrunkenKnitwits URL segment is the chapter's ShortName (derived from the name).
        var owner = await Provisioning.NewAccount("dk-chapter-owner");
        var chapterName = $"e2edk{Guid.NewGuid():N}";
        await Provisioning.SeedDrunkenKnitwitsChapter(owner, chapterName);
        var shortName = chapterName.ToLowerInvariant();

        var email = TestAccounts.NewEmailAddress();
        var password = $"E2e!{Guid.NewGuid():N}Zz9";

        // Act - join (= sign up), activate via the emailed token, then log in - all chapter-scoped.
        await new DrunkenKnitwitsJoinPage(Page).Join(shortName, "E2E", "Test", email);

        var token = await ActivationTokenDataHelper.GetActivationToken(email);
        await new DrunkenKnitwitsActivatePage(Page).Activate(shortName, token, password);

        await new DrunkenKnitwitsLoginPage(Page).LogIn(shortName, email, password);

        // Assert - logged in (not bounced back to the login page). The chapter login URL is PascalCased,
        // so compare case-insensitively.
        Page.Url.Should().NotContainEquivalentOf("/account/login");

        // Assert - joining sent the activation email, recorded in SentEmails as it passed through the
        // email client.
        var subjects = await SentEmailDataHelper.GetSubjects(email, expectedCount: 1);
        subjects.Should().Contain(
            x => x.Contains("Activate", StringComparison.OrdinalIgnoreCase),
            $"No activation email was sent. Subjects sent to {email}: [{string.Join(", ", subjects)}]");
    }

    [Test]
    public async Task JoinChapter_ChapterAtItsMemberLimit_IsRefusedAndCreatesNoAccount()
    {
        /* Arrange - a chapter whose owner is on a subscription allowing one member. Creating a chapter makes
           its owner an approved member, so that one place is already taken and the chapter is full before
           anybody tries to join. Signing up on DrunkenKnitwits *is* joining, so the same limit applies here as
           it does to a member who already has an account. */
        var subscription = await Provisioning.EnsureMemberLimitedSiteSubscription();

        var owner = await Provisioning.NewAccount("dk-full-chapter-owner");
        var chapterName = $"e2edkfull{Guid.NewGuid():N}";
        await Provisioning.SeedDrunkenKnitwitsChapter(owner, chapterName);
        var shortName = chapterName.ToLowerInvariant();

        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.EnsureActive(ownerId, subscription.Id, subscription.PriceId);

        var email = TestAccounts.NewEmailAddress();

        // Act
        var joinPage = new DrunkenKnitwitsJoinPage(Page);
        var joined = await joinPage.TryJoinWithProperties(
            shortName, "E2E", "Test", email, new Dictionary<Guid, string>());

        // Assert - refused, and told why.
        joined.Should().BeFalse("the chapter has no room for another member");
        (await joinPage.HasFeedback("This group is not able to welcome any new members"))
            .Should().BeTrue();

        /* Assert - refused before anything was written. No account is the decisive check: there is nothing to
           activate, and nothing for the activation email to be sent to. */
        (await Members.Exists(email)).Should().BeFalse();
    }
}
