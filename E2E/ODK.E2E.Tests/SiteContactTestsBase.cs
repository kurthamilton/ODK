using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Contacting the site, written once and run against both platforms: where the footer's Contact link goes
/// for a visitor and for a signed-in member, and how a site admin's answer is attributed in the member's
/// own thread. Concrete per-platform fixtures supply the platform base URL + category, the platform's
/// conversations URL, and how a member of that platform is provisioned.
/// </summary>
public abstract class SiteContactTestsBase : OdkPageTest
{
    /// <summary>What the member says, distinctive enough to tell their message from the answer to it.</summary>
    private const string Asked = "Please could somebody take a look at this.";

    /// <summary>What the site admin replies.</summary>
    private const string Answered = "Somebody has taken a look at this.";

    /// <summary>
    /// A page carrying the footer. The site home is the one page both platforms render for anyone, signed
    /// in or not, without a group behind it.
    /// </summary>
    private const string FooterPage = "/";

    /// <summary>How the site side of a thread is attributed to the member reading it.</summary>
    private const string SiteAdminName = "Site Admin";

    /// <summary>
    /// The name every provisioned test account carries, the site admin's included - so asserting it is
    /// absent is what distinguishes the site's attribution from the account that actually replied.
    /// </summary>
    private const string TestAccountName = "E2E Test";

    /// <summary>Where the footer's Contact link sends a signed-in member on this platform.</summary>
    private protected abstract string ConversationsPath { get; }

    [Test]
    public async Task FooterContactLink_NotSignedIn_OpensSiteContactForm()
    {
        // Arrange - a visitor on a page carrying the footer.
        await Page.Navigate(FooterPage);

        // Act - follow the footer's Contact link.
        await new SiteFooter(Page).ClickContact();

        // Assert - the site's contact form, which is what somebody with no thread to add to gets.
        CurrentPath().Should().Be("/contact");
        (await new SiteContactPage(Page).HasContactForm()).Should().BeTrue(
            "the contact page should render its form");
    }

    [Test]
    public async Task FooterContactLink_SignedIn_OpensMemberConversations()
    {
        // Arrange - a signed-in member on a page carrying the footer.
        var member = await NewMember();
        await LogIn(member);
        await Page.Navigate(FooterPage);

        // Act - follow the footer's Contact link.
        await new SiteFooter(Page).ClickContact();

        // Assert - their own conversations, so contacting the site is a thread they can pick up later
        // rather than a message they never see again.
        CurrentPath().Should().Be(ConversationsPath);
        (await new SiteConversationsPage(Page).HasStartConversationForm()).Should().BeTrue(
            "the conversations page should offer a way to open a thread");
    }

    [Test]
    public async Task SiteAdminReply_InMemberThread_IsAttributedToSiteAdmin()
    {
        // Arrange - a member with a thread of their own, opened through the UI.
        var member = await NewMember();
        var subject = $"E2E site contact {Guid.NewGuid():N}";
        await LogIn(member);

        var conversations = new SiteConversationsPage(Page);
        await conversations.Open(ConversationsPath);
        await conversations.StartConversation(subject, Asked);
        var conversationId = await conversations.OpenConversation(subject);

        // Act - a site admin answers it from the site-admin area, on their own browser.
        await Provisioning.ReplyToSiteConversationAsSiteAdmin(conversationId, Answered, PlatformBaseUrl);

        // Assert - the thread reads as the member talking to the site. Each message is paired with the words
        // it sits above, so the attributions are read off the messages they belong to rather than in the
        // abstract: the member's own is theirs, and the answer is the site's, because which admin picked the
        // conversation up is not theirs to know. Every test account is named TestAccountName, so that name
        // appearing anywhere in the thread is the failure.
        var conversation = new SiteConversationPage(Page);
        await conversation.Reload();

        var messages = await conversation.GetMessages();
        messages.Should().HaveCount(2);
        messages[0].Should().Contain(Asked).And.Contain("You");
        messages[1].Should().Contain(Answered).And.Contain(SiteAdminName);
        messages.Should().NotContainMatch($"*{TestAccountName}*");
    }

    /// <summary>
    /// A fresh member of this platform who can sign in. Fresh rather than shared: each of these tests
    /// starts a thread of the member's own, which is state a shared account would carry into the next.
    /// </summary>
    private protected abstract Task<TestAccount> NewMember();

    /// <summary>The path of the page currently on screen, without the platform's host or any query.</summary>
    private string CurrentPath() => new Uri(Page.Url).AbsolutePath;

    /// <summary>
    /// Signs the given member in on the test's own browser. Both platforms expose the site-level login,
    /// so this needs no platform hook.
    /// </summary>
    private Task LogIn(TestAccount member) => new LoginPage(Page).LogIn(member.Email, member.Password);
}
