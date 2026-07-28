using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Smoke tests for the member-facing chapter/profile pages that exist on BOTH platforms: each navigates
/// to one page as a chapter member and asserts it returns HTTP 200 and renders its expected content.
/// Pages that exist on only one platform are covered by <c>[Test]</c> methods on the concrete fixtures.
/// Concrete fixtures supply the platform's shared context, route builder, and the two per-platform
/// content anchors (home + profile); the rest are shared partials, so their anchors live here.
/// </summary>
public abstract class MemberFacingPageTestsBase : OdkPageTest
{
    // Shared-partial anchors - identical markup on both platforms.
    private protected const string ContactAnchor = "text=Send us a message";
    private protected const string EventDetailAnchor = "text=Back to events";
    private protected const string EventsListAnchor = "h1:has-text('Events'), h2:has-text('Events')";
    private protected const string MembersListAnchor = ".member-tile";

    private protected abstract string HomeAnchor { get; }

    private protected abstract string ProfileAnchor { get; }

    [Test]
    public Task ChapterHome_ReturnsOkAndRendersContent()
        => CheckPage((routes, context) => routes.Home, HomeAnchor);

    [Test]
    public Task ContactPage_ReturnsOkAndRendersContent()
        // The contact form is guest-facing - a logged-in member is redirected to their conversations - so
        // this one runs unauthenticated.
        => CheckPage((routes, context) => routes.Contact, ContactAnchor, asMember: false);

    [Test]
    public Task EventDetailPage_ReturnsOkAndRendersContent()
        => CheckPage((routes, context) => routes.EventPage(context.Event.Shortcode), EventDetailAnchor);

    [Test]
    public Task EventsListPage_ReturnsOkAndRendersContent()
        => CheckPage((routes, context) => routes.EventsList, EventsListAnchor);

    [Test]
    public Task MembersListPage_ReturnsOkAndRendersContent()
        => CheckPage((routes, context) => routes.MembersList, MembersListAnchor);

    [Test]
    public Task ProfilePage_ReturnsOkAndRendersContent()
        => CheckPage((routes, context) => routes.ProfileUpdate, ProfileAnchor);

    private protected async Task CheckPage(
        Func<PlatformRoutes, SmokeContext, string> url, string anchor, bool asMember = true)
    {
        // Arrange - the shared published chapter; log in as a member unless the page is guest-facing.
        var context = await SharedContext();
        var target = url(RoutesFor(context.Group), context);
        if (asMember)
        {
            await new LoginPage(Page).LogIn(context.Member.Email, context.Member.Password);
        }

        // Act - navigate to the page.
        var page = new MemberFacingPage(Page);
        var status = await page.Open(target);

        // Assert - it renders: 200 + its expected content.
        status.Should().Be(200, $"GET {target} should return 200");
        (await page.IsVisible(anchor)).Should().BeTrue($"expected content '{anchor}' to render at {target}");
    }

    private protected abstract PlatformRoutes RoutesFor(TestGroup group);

    private protected abstract Task<SmokeContext> SharedContext();
}
