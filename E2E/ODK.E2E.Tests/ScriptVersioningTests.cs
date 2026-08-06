using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Cache-busting (<c>asp-append-version</c>) on the scripts rendered by the shared partials under
/// <c>Views/**</c>. Those partials sit outside <c>Pages/**</c> and so depend on
/// <c>Views/_ViewImports.cshtml</c> registering <c>ScriptTagHelper</c>; without it the attribute is emitted
/// into the HTML as a literal, inert attribute and the scripts load unversioned, letting a browser serve
/// stale JS after a deploy. That failure is invisible on the page - the scripts still load - so it needs an
/// explicit test.
///
/// <c>/account/create</c> renders three of those partials (<c>_Imaging</c>, <c>_GoogleLocation</c>,
/// <c>_OAuth</c>) and is anonymous, which makes it the cheapest place to check: no login, no provisioning,
/// no payment provider.
/// </summary>
[TestFixture]
public class ScriptVersioningTests : DefaultPageTest
{
    private static readonly string[] PartialScripts =
    [
        "odk.imaging.js",
        "odk.google.places.js",
        "odk.oauth.js",
        "odk.google.oauth.js"
    ];

    [Test]
    public async Task SignUpPage_ScriptsRenderedByViewsPartials_AreVersioned()
    {
        // Arrange / Act - the page is anonymous, so rendering it is the whole exercise.
        await Page.Navigate("/account/create");

        // Assert - each script a Views/** partial renders carries a version query.
        var sources = await Page.GetScriptSources();
        foreach (var script in PartialScripts)
        {
            var src = sources.FirstOrDefault(x => x.Contains(script, StringComparison.OrdinalIgnoreCase));
            src.Should().NotBeNull($"/account/create should render {script}");
            src.Should().Contain("?v=", $"{script} should be cache-busted by asp-append-version");
        }

        // And no asp-* attribute survived into the rendered HTML. This is what an unregistered tag helper
        // leaves behind, so it catches the same regression on any partial - not just the four above.
        (await Page.HasUnprocessedTagHelperAttribute("asp-append-version")).Should().BeFalse(
            "asp-append-version should be consumed by ScriptTagHelper, not rendered as an attribute");
    }
}
