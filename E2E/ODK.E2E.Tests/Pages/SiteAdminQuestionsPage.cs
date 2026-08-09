using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site-admin FAQ pages (<c>/siteadmin/questions</c>), where a site admin manages the questions shown
/// on the About page. Requires the logged-in member to be a site admin (<c>Members.SuperAdmin = 1</c>).
/// Questions belong to the platform the request was made on, so whichever base URL the fixture drives
/// decides which platform's FAQ is being edited.
/// </summary>
internal class SiteAdminQuestionsPage
{
    private readonly IPage _page;

    public SiteAdminQuestionsPage(IPage page)
    {
        _page = page;
    }

    public async Task Create(string question, string answer)
    {
        await _page.Navigate("/siteadmin/questions/new");

        await _page.FillAsync("#Question", question);
        await FillAnswer(answer);

        await _page.ClickAsync("form[action='/siteadmin/questions/new'] button[type='submit']");
        await _page.WaitForLoadStateAsync();
    }

    public async Task Delete(Guid questionId)
    {
        await _page.Navigate("/siteadmin/questions");

        // The delete button is wrapped in a confirm modal, so the submit only happens once it's accepted.
        await _page.ClickAsync($"form[action='/siteadmin/questions/{questionId}/delete'] button");
        await _page.AcceptConfirm();
        await _page.WaitForLoadStateAsync();
    }

    public async Task<bool> IsListed(string question)
    {
        await _page.Navigate("/siteadmin/questions");
        return await _page.GetByRole(AriaRole.Link, new() { Name = question }).CountAsync() > 0;
    }

    public async Task MoveUp(Guid questionId)
    {
        await _page.Navigate("/siteadmin/questions");

        await _page.ClickAsync($"form[action='/siteadmin/questions/{questionId}/move/up'] button");
        await _page.WaitForLoadStateAsync();
    }

    public async Task Update(Guid questionId, string question, string answer)
    {
        await _page.Navigate($"/siteadmin/questions/{questionId}");

        await _page.FillAsync("#Question", question);
        await FillAnswer(answer);

        await _page.ClickAsync($"form[action='/siteadmin/questions/{questionId}'] button[type='submit']");
        await _page.WaitForLoadStateAsync();
    }

    private Task FillAnswer(string answer) => _page.SetHtmlEditor("Answer", answer);
}
