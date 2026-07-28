using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Member-facing chapter/profile page smoke tests on the <c>DrunkenKnitwits</c> platform. The shared
/// common-page scenarios live in <see cref="MemberFacingPageTestsBase"/>; this fixture adds the pages
/// that exist only on DrunkenKnitwits (the About page).
/// </summary>
[TestFixture]
[Category("DrunkenKnitwits")]
public class DrunkenKnitwitsMemberFacingPageTests : MemberFacingPageTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DrunkenKnitwitsBaseUrl;

    private protected override string HomeAnchor => "[data-odk-component='_ChapterSidebar']";

    private protected override string ProfileAnchor => "text=Date joined";

    [Test]
    public Task AboutPage_ReturnsOkAndRendersContent()
        => CheckPage((routes, context) => $"/{context.Group.Name.ToLowerInvariant()}/about", "text=Frequently asked questions");

    private protected override PlatformRoutes RoutesFor(TestGroup group) => PlatformRoutes.DrunkenKnitwits(group);

    private protected override Task<SmokeContext> SharedContext() => SharedSmokeContent.DrunkenKnitwits();
}
