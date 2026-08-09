using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

internal static class PageExtensions
{
    /// <summary>
    /// Accepts the shared confirmation dialog - the _Confirm component plus _ConfirmModal, which replaced
    /// the native <c>window.confirm</c>. A form rendering _Confirm does not submit when its button is
    /// clicked: the submit is intercepted and replayed only once this is accepted, so any test driving such
    /// a form must call this after the click. Playwright waits for the dialog to be actionable, which
    /// covers the fade-in.
    /// </summary>
    internal static Task AcceptConfirm(this IPage page)
        => page.ClickAsync("#confirm-modal [data-odk-confirm-accept]");

    /// <summary>
    /// Navigates to a relative path. The absolute host comes from the browser context's
    /// <c>BaseURL</c> (set per platform by the test's base class / by provisioning), so the same page
    /// objects work against whichever platform the fixture targets. Returns the response so a caller can
    /// assert the status - a page that legitimately 404s needs to tell the difference between "not found"
    /// and "found but empty"; callers that don't care can ignore it.
    /// </summary>
    internal static Task<IResponse?> Navigate(this IPage page, string path)
        => page.GotoAsync(path);

    /// <summary>
    /// Sets a value on a flatpickr-enhanced date input (<c>[data-datepicker]</c>). The visible input is
    /// read-only, so it can't be typed into - drive the flatpickr instance directly. Waits for flatpickr
    /// to attach first. <paramref name="value"/> is in the input's display format (<c>dd/MM/yyyy HH:mm</c>).
    /// </summary>
    /// <summary>
    /// Sets the content of a TinyMCE-enhanced textarea (<c>[data-html-editor]</c>, initialised globally
    /// from the layout). The editor hides the original textarea and edits inside an iframe, so it can't be
    /// filled directly - set the content through the editor's API and <c>save()</c> it back to the
    /// textarea, which is what client validation reads and the form posts. Waits for initialisation first,
    /// since the editor is created asynchronously after load.
    /// </summary>
    /// <param name="elementId">The id of the underlying textarea, which is also the editor's id.</param>
    internal static async Task SetHtmlEditor(this IPage page, string elementId, string value)
    {
        await page.WaitForFunctionAsync(
            "id => { const ed = window.tinymce && window.tinymce.get(id); return !!ed && ed.initialized === true; }",
            elementId);

        await page.EvaluateAsync(
            """
            ({ id, value }) => {
                const editor = window.tinymce.get(id);
                editor.setContent(value);
                editor.save();
            }
            """,
            new { id = elementId, value });
    }

    internal static async Task SetDatePicker(this IPage page, string selector, string value)
    {
        await page.WaitForFunctionAsync(
            "selector => { const el = document.querySelector(selector); return !!(el && el._flatpickr); }",
            selector);

        await page.EvaluateAsync(
            """
            ({ selector, value }) => {
                document.querySelector(selector)._flatpickr.setDate(value, true, 'd/m/Y H:i');
            }
            """,
            new { selector, value });
    }

    /// <summary>
    /// Fills a chapter-property answer on a form built from <c>_ChapterProfileForm</c> (the join and
    /// profile-update forms). Those forms render property fields indexed by position
    /// (<c>Properties[i].Value</c>) with a sibling hidden <c>Properties[i].ChapterPropertyId</c>, so this
    /// finds the index whose hidden id matches and fills that row's value. Works for text properties.
    /// </summary>
    internal static async Task FillChapterProperty(this IPage page, Guid chapterPropertyId, string value)
    {
        var prefix = await page.EvaluateAsync<string?>(
            """
            guid => {
                const hidden = document.querySelector(`input[name$='.ChapterPropertyId'][value='${guid}']`);
                return hidden ? hidden.name.slice(0, hidden.name.lastIndexOf('.')) : null;
            }
            """,
            chapterPropertyId.ToString());

        if (prefix == null)
        {
            throw new InvalidOperationException(
                $"No chapter-property field found for property '{chapterPropertyId}' on the form.");
        }

        await page.FillAsync($"[name='{prefix}.Value']", value);
    }

    /// <summary>
    /// Whether a chapter-property field for the given property is present on a <c>_ChapterProfileForm</c>
    /// form (detected by its hidden <c>ChapterPropertyId</c> input).
    /// </summary>
    internal static async Task<bool> HasChapterProperty(this IPage page, Guid chapterPropertyId)
        => await page.Locator($"input[name$='.ChapterPropertyId'][value='{chapterPropertyId}']").CountAsync() > 0;

    /// <summary>
    /// The <c>src</c> of every script on the page, as authored rather than resolved - so a cache-busting
    /// version query is visible.
    /// </summary>
    internal static async Task<string[]> GetScriptSources(this IPage page)
        => await page.EvaluateAsync<string[]>(
            "() => Array.from(document.querySelectorAll('script[src]')).map(x => x.getAttribute('src'))");

    /// <summary>
    /// Whether any element still carries the given <c>asp-*</c> attribute. A tag helper that ran consumes
    /// its attribute; one that was never registered for that view's directory leaves it behind in the HTML,
    /// inert.
    /// </summary>
    internal static async Task<bool> HasUnprocessedTagHelperAttribute(this IPage page, string name)
        => await page.Locator($"[{name}]").CountAsync() > 0;

    /// <summary>
    /// Selects a single value on a SlimSelect-enhanced <c>&lt;select&gt;</c> (<c>[data-select]</c>/
    /// <c>[data-searchable]</c>). SlimSelect hides the native control, so Playwright's SelectOption can't
    /// see it - set the native value (what actually posts) and raise the events SlimSelect and the
    /// validator listen for (<c>change</c> to satisfy validation, the app's <c>odk:change</c> to sync the
    /// widget UI).
    /// </summary>
    internal static Task SetEnhancedSelect(this IPage page, string selector, string value)
        => page.EvaluateAsync(
            """
            ({ selector, value }) => {
                const el = document.querySelector(selector);
                if (!el) throw new Error('No element matched ' + selector);
                el.value = value;
                el.dispatchEvent(new Event('change', { bubbles: true }));
                el.dispatchEvent(new CustomEvent('odk:change', { detail: { values: [value] } }));
            }
            """,
            new { selector, value });
}
