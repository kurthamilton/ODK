using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// A group's edit page for one email template - Default <c>/my/groups/{chapterId}/emails/{type}</c>,
/// DrunkenKnitwits <c>/{chapterName}/admin/chapter/emails/{type}</c>. The caller passes the
/// platform-correct URL.
/// <para>
/// Subject and body are customised independently. Each field shows what the group sends - its own wording
/// where it customises, the site's default otherwise - and a field left on the default is disabled, so it
/// posts nothing and stays inheriting. A "Customise" switch per field is what releases it.
/// </para>
/// </summary>
internal class ChapterEmailAdminPage
{
    private const string ContentField = "#email-content";

    private const string ContentToggle = "#override-content";

    private const string SubjectField = "#Subject";

    private const string SubjectToggle = "#override-subject";

    private readonly IPage _page;

    public ChapterEmailAdminPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Whether the page badges the email as customised. Matched exactly: the recipient badge sits beside it,
    /// and a substring match on "Custom" would also hit an upgrade prompt naming the Custom emails feature.
    /// </summary>
    public Task<bool> IsCustomised() =>
        _page.Locator(".badge").GetByText("Custom", new() { Exact = true }).IsVisibleAsync();

    /// <summary>
    /// What the templates list reports the group has customised for the email at <paramref name="listUrl"/>'s
    /// first row - "Default", or the fields it overrides ("Subject", "Body", "Subject, Body").
    /// </summary>
    public async Task<string> GetListedCustomFields(string listUrl)
    {
        await _page.Navigate(listUrl);
        return (await _page.InnerTextAsync("tbody tr:first-child td:last-child")).Trim();
    }

    public Task Open(string emailUrl) => _page.Navigate(emailUrl);

    /// <summary>
    /// Opens the first template on the group's list and returns its path, for a test to revisit. Which
    /// template that is comes from the app: the list carries the group emails, which are exactly the ones a
    /// group may override. A group sends email, so the list always has rows.
    /// </summary>
    public async Task<string> OpenFirstEmail(string listUrl)
    {
        await _page.Navigate(listUrl);
        await _page.ClickAsync("tbody tr:first-child td:first-child a");
        await _page.WaitForLoadStateAsync();

        return new Uri(_page.Url).PathAndQuery;
    }

    /// <summary>
    /// Turns customisation off for both fields and saves, which is how a group goes back to the default.
    /// </summary>
    public async Task RestoreDefaults(string emailUrl)
    {
        await Open(emailUrl);
        await SetToggle(SubjectToggle, on: false);
        await SetToggle(ContentToggle, on: false);
        await Save();
    }

    /// <summary>
    /// Customises whichever of the two is given and saves. Passing null for one leaves it as it is - so
    /// setting only the subject is what checks the body carries on inheriting.
    /// </summary>
    public async Task SetCustomWording(string emailUrl, string? subject = null, string? htmlContent = null)
    {
        await Open(emailUrl);

        if (subject != null)
        {
            await SetToggle(SubjectToggle, on: true);
            await _page.FillAsync(SubjectField, subject);
        }

        if (htmlContent != null)
        {
            await SetToggle(ContentToggle, on: true);
            await SetContent(htmlContent);
        }

        await Save();
    }

    /// <summary>Sends a test of this email to the signed-in admin, via the form's own Send test button.</summary>
    public async Task SendTest(string emailUrl)
    {
        await Open(emailUrl);
        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("button:has-text('Send test')"),
            r => r.Request.Method == "POST" && r.Url.Contains("/test"));
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// What the body field holds. Read through the Ace editor where one is mounted: it owns the visible
    /// text, and the textarea behind it is moved out of sight.
    /// </summary>
    public Task<string> GetContent() => _page.EvaluateAsync<string>(
        """
        (selector) => {
            const field = document.querySelector(selector);
            const editor = window.odk.codeEditor?.get(field);
            return editor ? editor.getValue() : field.value;
        }
        """,
        ContentField);

    public Task<string> GetSubject() => _page.InputValueAsync(SubjectField);

    /// <summary>
    /// Whether the body may be typed into. False where the group is on the default, or where its
    /// subscription does not cover custom emails.
    /// </summary>
    /// <remarks>
    /// Read as a property rather than through Playwright's IsEditable: the textarea is deliberately moved out
    /// of sight behind the editor, and the state getters are built around elements a user can see.
    /// </remarks>
    public Task<bool> IsContentEditable() => _page.EvaluateAsync<bool>(
        "(selector) => !document.querySelector(selector).disabled", ContentField);

    public Task<bool> IsSubjectEditable() => _page.Locator(SubjectField).IsEditableAsync();

    /// <summary>Whether the body's Customise switch can be operated at all.</summary>
    public Task<bool> IsContentToggleEnabled() => _page.Locator(ContentToggle).IsEnabledAsync();

    private async Task Save()
    {
        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("button:has-text('Update')"),
            r => r.Request.Method == "POST");
        await _page.WaitForLoadStateAsync();
    }

    // Ace owns the box the user types in, so the textarea cannot be filled directly - see
    // odk.code-editor.js. Setting through the editor writes the textarea and raises the change the
    // server-side HTML check listens for, exactly as typing would.
    private async Task SetContent(string htmlContent)
    {
        await _page.EvaluateAsync(
            """
            ([selector, value]) => {
                const field = document.querySelector(selector);
                const editor = window.odk.codeEditor?.get(field);
                if (editor) {
                    editor.setValue(value, -1);
                } else {
                    field.value = value;
                }

                field.dispatchEvent(new Event('change', { bubbles: true }));
            }
            """,
            new[] { ContentField, htmlContent });
    }

    // The switches are driven rather than checked directly so the script that enables the field runs -
    // Check/Uncheck raise change, which is what odk.field-override.js listens for.
    private async Task SetToggle(string selector, bool on)
    {
        var toggle = _page.Locator(selector);
        if (await toggle.IsCheckedAsync() == on)
        {
            return;
        }

        if (on)
        {
            await toggle.CheckAsync();
        }
        else
        {
            await toggle.UncheckAsync();
        }
    }
}
