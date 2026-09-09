using System.Text;
using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member-import admin page (<c>.../members/import</c>): upload a CSV, which the group then holds, and
/// send the invites for everyone it holds who is ready. Platform-agnostic - the caller supplies the
/// platform-correct URLs from <see cref="PlatformRoutes"/>.
/// </summary>
internal class MemberImportAdminPage
{
    private readonly IPage _page;

    public MemberImportAdminPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Uploads the given rows as a CSV and sends the invites. Returns once sending has redirected to the
    /// invited page, which is the app's success path: by then the invites are written, though the emails
    /// follow on a background job - poll for those.
    /// </summary>
    public async Task Import(
        string importUrl,
        string invitedUrl,
        IReadOnlyCollection<MemberImportRow> rows)
    {
        await _page.Navigate(importUrl);

        await _page.SetInputFilesAsync("#File", new FilePayload
        {
            Name = "members.csv",
            MimeType = "text/csv",
            Buffer = Encoding.UTF8.GetBytes(BuildCsv(rows))
        });

        /* The upload posts to a controller and redirects back to this same address, so nothing about the URL
           says it has happened - the committed document is the only signal. */
        await _page.ClickAndWaitForDocument(
            "[data-odk-component='_ImportMembersUpload'] button[type='submit']");

        await _page.ClickAsync("button:has-text('Send invites')");

        /* Sending lands on a different address, so the URL does commit the navigation. EndsWith, not
           Contains: the two URLs share a prefix, so Contains would match the page we are leaving. */
        await _page.WaitForURLAsync(
            url => url.EndsWith(invitedUrl, StringComparison.OrdinalIgnoreCase),
            new() { Timeout = 15000 });
    }

    // The header names the properties MemberImportCsvRow binds, which is what the app's CSV reader matches on.
    private static string BuildCsv(IReadOnlyCollection<MemberImportRow> rows)
    {
        var lines = new List<string> { "FirstName,LastName,EmailAddress" };
        lines.AddRange(rows.Select(x => $"{x.FirstName},{x.LastName},{x.EmailAddress}"));
        return string.Join("\r\n", lines);
    }
}
