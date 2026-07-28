using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Member-facing chapter/profile page smoke tests on the <c>Default</c> platform. The shared common-page
/// scenarios live in <see cref="MemberFacingPageTestsBase"/>; this fixture adds the pages that exist only
/// on Default (FAQ, past events).
/// </summary>
[TestFixture]
[Category("Default")]
public class MemberFacingPageTests : MemberFacingPageTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    private protected override string HomeAnchor => "text=Organisers";

    private protected override string ProfileAnchor => "form[action$='/profile']";

    [Test]
    public Task FaqPage_ReturnsOkAndRendersContent()
        => CheckPage((routes, context) => $"/groups/{context.Group.Slug}/faq", "text=Frequently asked questions");

    [Test]
    public Task PastEventsPage_ReturnsOkAndRendersContent()
        => CheckPage((routes, context) => $"/groups/{context.Group.Slug}/events/past", "text=There are no past events.");

    private protected override PlatformRoutes RoutesFor(TestGroup group) => PlatformRoutes.Default(group);

    private protected override Task<SmokeContext> SharedContext() => SharedSmokeContent.Default();
}
