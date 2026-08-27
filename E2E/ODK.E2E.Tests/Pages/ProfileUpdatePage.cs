using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member's own chapter-profile update form (Default <c>/groups/{slug}/profile</c>, DrunkenKnitwits
/// <c>/{chapterName}/account/profile</c>). It renders the same <c>_ChapterProfileForm</c> property fields
/// as the join form but excludes ApplicationOnly properties, and posts to the shared
/// <c>/groups/{chapterId}/profile</c> controller endpoint.
/// </summary>
internal class ProfileUpdatePage
{
    private readonly IPage _page;

    public ProfileUpdatePage(IPage page)
    {
        _page = page;
    }

    /// <summary>Whether a field for the given chapter property is present on the update form.</summary>
    public async Task<bool> HasProperty(string profileUrl, Guid chapterPropertyId)
    {
        await _page.Navigate(profileUrl);
        return await _page.HasChapterProperty(chapterPropertyId);
    }

    public async Task UpdateProperty(string profileUrl, Guid chapterPropertyId, string value)
    {
        await _page.Navigate(profileUrl);
        await _page.FillChapterProperty(chapterPropertyId, value);

        await _page.RunAndWaitForDocument(() => _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("button:has-text('Update')"),
            r => r.Request.Method == "POST" && r.Url.Contains("/profile")));
    }
}
