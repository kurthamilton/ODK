using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

internal static class PageExtensions
{
    /// <summary>
    /// Navigates to a relative path. The absolute host comes from the browser context's
    /// <c>BaseURL</c> (set per platform by the test's base class / by provisioning), so the same page
    /// objects work against whichever platform the fixture targets.
    /// </summary>
    internal static Task Navigate(this IPage page, string path)
        => page.GotoAsync(path);

    /// <summary>
    /// Sets a value on a flatpickr-enhanced date input (<c>[data-datepicker]</c>). The visible input is
    /// read-only, so it can't be typed into - drive the flatpickr instance directly. Waits for flatpickr
    /// to attach first. <paramref name="value"/> is in the input's display format (<c>dd/MM/yyyy HH:mm</c>).
    /// </summary>
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
