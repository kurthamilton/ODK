using System.Text;
using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member-import admin wizard (<c>.../members/import</c>): upload a CSV, review what it will do, then
/// confirm. Platform-agnostic - the caller supplies the platform-correct URLs from
/// <see cref="PlatformRoutes"/>.
/// </summary>
internal class MemberImportAdminPage
{
    private readonly IPage _page;

    public MemberImportAdminPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Uploads the given rows as a CSV and confirms the import. Returns once the confirm has redirected to
    /// the members list, which is the app's success path: by then the invitations are written, though the
    /// emails follow on a background job - poll for those.
    /// </summary>
    public async Task Import(
        string importUrl,
        string membersUrl,
        IReadOnlyCollection<MemberImportRow> rows)
    {
        await _page.Navigate(importUrl);

        await _page.SetInputFilesAsync("#File", new FilePayload
        {
            Name = "members.csv",
            MimeType = "text/csv",
            Buffer = Encoding.UTF8.GetBytes(BuildCsv(rows))
        });

        /* The wizard's Next button submits the upload form by id (data-submit) and the POST re-renders the
           page on the review step, so wait for the confirm button to exist rather than for the click. */
        await _page.ClickAsync("button[data-submit='#upload-form']");
        await _page.WaitForSelectorAsync("button:has-text('Import')", new() { Timeout = 15000 });

        await _page.ClickAsync("button:has-text('Import')");

        // EndsWith, not Contains: the import URL contains the members URL as a prefix, so Contains would
        // match the page we are leaving.
        await _page.WaitForURLAsync(
            url => url.EndsWith(membersUrl, StringComparison.OrdinalIgnoreCase),
            new() { Timeout = 15000 });
    }

    // The header names the properties MemberImportModel binds, which is what the app's CSV reader matches on.
    private static string BuildCsv(IReadOnlyCollection<MemberImportRow> rows)
    {
        var lines = new List<string> { "FirstName,LastName,EmailAddress" };
        lines.AddRange(rows.Select(x => $"{x.FirstName},{x.LastName},{x.EmailAddress}"));
        return string.Join("\r\n", lines);
    }
}
