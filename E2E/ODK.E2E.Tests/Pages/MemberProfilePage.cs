using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// A member's profile page, shown to fellow group members (Default <c>/groups/{slug}/members/{id}</c>,
/// DrunkenKnitwits <c>/{chapterName}/members/{id}</c>). Each answered property renders as a
/// <c>.form-group</c> with a <c>.form-label</c> (the property Label) and a <c>.form-control-plaintext</c>
/// value, in the properties' display order. ApplicationOnly and empty-value properties are omitted, and a
/// trailing non-property "Date joined" block is always appended.
/// </summary>
internal class MemberProfilePage
{
    private readonly IPage _page;

    public MemberProfilePage(IPage page)
    {
        _page = page;
    }

    /// <summary>The value shown for the property with the given label, or null if it isn't shown.</summary>
    public async Task<string?> GetAnswer(string memberPageUrl, string label)
    {
        await _page.Navigate(memberPageUrl);
        return await _page.EvaluateAsync<string?>(
            """
            label => {
                for (const group of document.querySelectorAll('.form-group')) {
                    const l = group.querySelector('.form-label');
                    if (l && l.textContent.trim() === label) {
                        const v = group.querySelector('.form-control-plaintext');
                        return v ? v.textContent.trim() : null;
                    }
                }
                return null;
            }
            """,
            label);
    }

    /// <summary>The property labels shown on the page, in document (display) order.</summary>
    public async Task<IReadOnlyList<string>> GetLabelsInOrder(string memberPageUrl)
    {
        await _page.Navigate(memberPageUrl);
        var labels = await _page.EvalOnSelectorAllAsync<string[]>(
            ".form-group .form-label",
            "els => els.map(e => e.textContent.trim())");
        return labels;
    }

    /// <summary>Navigates to the member page and returns the HTTP status (Playwright doesn't throw on 4xx).</summary>
    public async Task<int> GetResponseStatus(string memberPageUrl)
    {
        var response = await _page.GotoAsync(memberPageUrl)
            ?? throw new InvalidOperationException($"No response navigating to '{memberPageUrl}'.");
        return response.Status;
    }
}
