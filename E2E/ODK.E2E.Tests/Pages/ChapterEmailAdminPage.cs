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

    // The toolbar buttons are icon-only, so there is no text to match them on.
    private const string PreviewButton = "[data-email-preview-url]";

    private const string PreviewFrame = "[data-email-preview-frame]";

    private const string PreviewSubject = "[data-email-preview-subject]";

    private const string SendTestButton = "[data-submit='#send-test-form']";

    private const string SubjectToggle = "#override-subject";

    private readonly IPage _page;

    public ChapterEmailAdminPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Whether the form shows the body as customised. The switches are the page's own record of that, so
    /// reading them back after a reload is what checks the form reflects what was stored.
    /// </summary>
    public Task<bool> IsContentCustomised() => _page.Locator(ContentToggle).IsCheckedAsync();

    /// <inheritdoc cref="IsContentCustomised" />
    public Task<bool> IsSubjectCustomised() => _page.Locator(SubjectToggle).IsCheckedAsync();

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
        await _page.ClickAndWaitForDocument("tbody tr:first-child td:first-child a");

        return new Uri(_page.Url).PathAndQuery;
    }

    /// <summary>
    /// Turns customisation off for both fields and saves, which is how a group goes back to the default.
    /// </summary>
    public Task RestoreDefaults(string emailUrl) => SetCustomised(emailUrl, subject: false, content: false);

    /// <summary>
    /// Sets whichever Customise switches are given and saves, without touching any wording. Passing null for
    /// one leaves that switch as it is.
    /// </summary>
    /// <remarks>
    /// The only way to drive a form whose fields are locked - a group without the custom emails feature can
    /// still operate the switches, and that is exactly when the wording cannot be typed.
    /// </remarks>
    public async Task SetCustomised(string emailUrl, bool? subject = null, bool? content = null)
    {
        await Open(emailUrl);

        if (subject != null)
        {
            await SetToggle(SubjectToggle, on: subject.Value);
        }

        if (content != null)
        {
            await SetToggle(ContentToggle, on: content.Value);
        }

        await Save();
    }

    /// <summary>
    /// Customises whichever of the two is given and saves. Passing null for one leaves it as it is - so
    /// setting only the subject is what checks the body carries on inheriting.
    /// </summary>
    public async Task SetCustomWording(string emailUrl, string? subject = null, string? htmlContent = null)
    {
        await Open(emailUrl);
        await EnterWording(subject, htmlContent);
        await Save();
    }

    /// <summary>
    /// Types wording into whichever of the two is given, releasing its Customise switch first, and stops
    /// short of saving - which is what lets a test check the preview against unsaved wording.
    /// </summary>
    public async Task EnterWording(string? subject = null, string? htmlContent = null)
    {
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
    }

    /// <summary>
    /// Opens the preview dialog for the wording the form currently holds and returns the whole email as its
    /// frame renders it, layout included.
    /// </summary>
    public async Task<string> Preview()
    {
        await _page.ClickAsync(PreviewButton);

        // The frame is filled from the preview response, so its content is empty until that arrives - the
        // dialog itself opens first.
        await _page.WaitForFunctionAsync(
            "selector => document.querySelector(selector)?.srcdoc?.length > 0", PreviewFrame);

        return await _page.GetAttributeAsync(PreviewFrame, "srcdoc") ?? string.Empty;
    }

    /// <summary>The resolved subject the preview reports, which is shown as text beside the frame.</summary>
    public Task<string> GetPreviewSubject() => _page.InnerTextAsync(PreviewSubject);

    /// <summary>
    /// Sends a test of this email to the signed-in admin, via the test button in the editor's toolbar.
    /// </summary>
    public async Task SendTest(string emailUrl)
    {
        await Open(emailUrl);
        await Submit(SendTestButton, r => r.Request.Method == "POST" && r.Url.Contains("/test"));
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

    /* Identified as the POST that navigates. Any-POST would also match the editor's HTML-check request - it
       posts the content to ValidateUrl whenever the body changes - and returning on that one hands back before
       the save has happened, so an assertion reads the database as it was. */
    private Task Save() => Submit(
        "button:has-text('Update')",
        r => r.Request.Method == "POST" && r.Request.ResourceType == "document");

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

    /* Clicks something that posts, and returns only once the document it redirects to has been committed.
       Waiting on the POST response alone is not enough: every form here is Post/Redirect/Get, and that
       response is the 302 - so it arrives while the redirected GET is still in flight. A caller that then
       navigates to the same URL has its navigation cut short by the one already running, which Playwright
       reports as "Navigation to X is interrupted by another navigation to X".

       The document is stamped before the click and waited on until the stamp has gone: a window property does
       not survive a navigation, so its absence is the redirected document having replaced the one that posted.

       Not a load-state wait, and not waiting for the redirected GET's response: a load state resolves against
       whichever document is current when it is called, and until the redirect commits that is still the
       document that posted - which reached every state long before the click - so the wait returns at once and
       guards nothing. The response arriving is likewise not the browser having committed what it carries. */
    private Task Submit(string selector, Func<IResponse, bool> posted)
        => _page.RunAndWaitForDocument(
            () => _page.RunAndWaitForResponseAsync(() => _page.ClickAsync(selector), posted));
}
