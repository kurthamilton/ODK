using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// Drives the Stripe Payment Element on the currently-loaded checkout page - the <c>_StripeCheckout</c>
/// component, which mounts the element inside <c>[data-odk-stripe-payment-element]</c> and pays with our own
/// <c>[data-odk-stripe-submit]</c> button. Uses the test card 4242 4242 4242 4242; on success Stripe
/// redirects the top frame off the checkout page. Completing the purchase is webhook-driven (poll the DB) -
/// this only gets the card accepted. Shared by the site- and chapter-subscription flows, which mount the
/// element identically and differ only in the URL the caller navigates to first.
/// <para>
/// Replaced a driver for Stripe's embedded Checkout page, which this integration no longer uses: the card
/// fields are now the Payment Element's (in a <c>js.stripe.com</c> iframe) rather than a hosted checkout
/// page's, there are no billing name/country fields unless we enable them, and - importantly - the submit
/// button is ours, on the page, not inside a Stripe frame.
/// </para>
/// <para>
/// Stripe renders lazily and can nest frames, so rather than assuming where the card field lands we scan
/// every frame for it, expanding the "Card" accordion item first if it isn't already open. If it never
/// appears, the failure dumps every Stripe frame's inputs to show what to use instead.
/// </para>
/// </summary>
internal static class StripeElementsCheckout
{
    // Matched on autocomplete as well as id: the attribute is part of the rendered card form's contract,
    // where Stripe's internal Field-* ids are not.
    private const string CardCvcSelector = "input[autocomplete='cc-csc'], #Field-cvcInput";
    private const string CardExpirySelector = "input[autocomplete='cc-exp'], #Field-expiryInput";
    private const string CardNumberSelector = "input[autocomplete='cc-number'], #Field-numberInput";
    private const string PostalCodeSelector = "input[autocomplete='postal-code'], #Field-postalCodeInput";

    private const string SubmitSelector = "[data-odk-stripe-submit]";

    private const string TestCardCvc = "123";
    private const string TestCardExpiry = "1234";
    private const string TestCardNumber = "4242424242424242";

    /// <summary>
    /// Pays the currently-loaded checkout with the Stripe test card, returning once Stripe has accepted it
    /// and redirected the top frame off the checkout page. Throws with a per-frame diagnostic if the card
    /// form never appears.
    /// </summary>
    public static async Task PayWithTestCard(IPage page)
    {
        // The payment method accordion plus the card form can run past the default viewport height, and
        // controls inside Stripe's iframe stay unclickable when they're outside it even after scrolling.
        await page.SetViewportSizeAsync(1280, 2400);

        // Card is usually the expanded method already; presence of its fields is the only reliable signal.
        // If absent, expand it - retrying, because Stripe re-renders the accordion as the wallet/Link
        // options finish loading and can collapse a just-opened panel.
        var cardFrame = await FindFrameWithCardField(page, 20);
        var expandOutcome = "(not attempted - card was already expanded)";
        for (var attempt = 0; attempt < 5 && cardFrame == null; attempt++)
        {
            expandOutcome = await ExpandCardAccordion(page);
            cardFrame = await FindFrameWithCardField(page, 8);
        }

        if (cardFrame == null)
        {
            throw new InvalidOperationException(await Diagnose(page) + " || Expand: " + expandOutcome);
        }

        // Type char by char so Stripe's field formatters run (FillAsync sets the value directly).
        await cardFrame.Locator(CardNumberSelector).First.PressSequentiallyAsync(TestCardNumber);
        await cardFrame.Locator(CardExpirySelector).First.PressSequentiallyAsync(TestCardExpiry);
        await cardFrame.Locator(CardCvcSelector).First.PressSequentiallyAsync(TestCardCvc);

        // A postal-code field only renders for some countries; fill it if present.
        var postalCode = cardFrame.Locator(PostalCodeSelector);
        if (await postalCode.CountAsync() > 0)
        {
            await postalCode.First.PressSequentiallyAsync("SW1A 1AA");
        }

        // Ours, on the page - and disabled until Stripe reports the form can be confirmed, so the click
        // auto-waits for that rather than needing a poll.
        await page.Locator(SubmitSelector).ClickAsync();

        // On success Stripe redirects the top frame off the checkout page (to the session's return_url).
        // Waiting for the checkout page to be left confirms the card was accepted; the DB poll verifies
        // completion.
        await page.WaitForURLAsync(url => !url.Contains("/checkout"), new() { Timeout = 30000 });
    }

    // Lists each Stripe frame's clickable controls, so the collapsed-state control that expands the card
    // form is visible in a failure.
    private static async Task<string> DescribeClickables(IPage page)
    {
        var parts = new List<string>();
        foreach (var frame in StripeFrames(page))
        {
            try
            {
                var items = await frame.Locator("[data-testid], button").EvaluateAllAsync<string[]>(
                    "els => els.map(function (e) { return e.tagName + " +
                    "(e.getAttribute('data-testid') ? '[testid=' + e.getAttribute('data-testid') + ']' : '') + " +
                    "(e.getAttribute('aria-label') ? '[aria=' + e.getAttribute('aria-label') + ']' : '') + " +
                    "(e.id ? '#' + e.id : ''); })");
                if (items.Length > 0)
                {
                    parts.Add("[" + string.Join(", ", items) + "]");
                }
            }
            catch (PlaywrightException)
            {
                // Cross-origin or detached frame; skip.
            }
        }

        return parts.Count > 0 ? string.Join(" || ", parts) : "(none found)";
    }

    private static async Task<string> Diagnose(IPage page)
    {
        var parts = new List<string>();
        foreach (var frame in StripeFrames(page))
        {
            try
            {
                var inputs = await frame.Locator("input, select").EvaluateAllAsync<string[]>(
                    "els => els.map(function (e) { return '#' + (e.id || '') + '[autocomplete=' + " +
                    "(e.getAttribute('autocomplete') || '') + ']'; })");
                if (inputs.Length > 0)
                {
                    parts.Add($"{frame.Url.Split('?')[0].Split('/').Last()} => [{string.Join(", ", inputs)}]");
                }
            }
            catch (PlaywrightException)
            {
                // Cross-origin or detached frame; skip.
            }
        }

        // An empty client secret means the server never created the session (e.g. Stripe rejected the
        // connected-account transfer destination) - check the app log rather than the test.
        var form = page.Locator("[data-odk-stripe-checkout]");
        var clientSecret = await form.CountAsync() > 0
            ? await form.GetAttributeAsync("data-odk-stripe-checkout") ?? "(attribute absent)"
            : "(no [data-odk-stripe-checkout] element)";

        var body = await page.InnerTextAsync("body");

        return "Card number field not found in any frame. " +
            $"data-odk-stripe-checkout='{clientSecret}' (empty => the server did not create a checkout " +
            "session). Stripe frame inputs: " + (parts.Count > 0 ? string.Join(" || ", parts) : "(none)") +
            " || Clickables: " + await DescribeClickables(page) +
            $" || Body: {body[..Math.Min(400, body.Length)]}";
    }

    // Clicks the "Card" accordion item to reveal the card form. The accordion lives inside the Payment
    // Element's iframe; a JS click dispatches straight to the element, bypassing viewport/actionability
    // while still firing Stripe's React onClick handler.
    private static async Task<string> ExpandCardAccordion(IPage page)
    {
        foreach (var frame in StripeFrames(page))
        {
            try
            {
                var button = frame.Locator(
                    "[data-testid='card-accordion-item-button'], " +
                    "button:has-text('Card'), [role='button']:has-text('Card')").First;

                if (await button.CountAsync() == 0)
                {
                    continue;
                }

                await button.EvaluateAsync("el => el.click()");
                return "clicked (js)";
            }
            catch (PlaywrightException)
            {
                // Frame navigated/detached mid-query; keep looking.
            }
        }

        return "card button absent";
    }

    private static async Task<IFrame?> FindFrameWithCardField(IPage page, int maxAttempts)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            foreach (var frame in StripeFrames(page))
            {
                try
                {
                    if (await frame.Locator(CardNumberSelector).CountAsync() > 0)
                    {
                        return frame;
                    }
                }
                catch (PlaywrightException)
                {
                    // Frame navigated/detached mid-query; ignore and keep scanning.
                }
            }

            await page.WaitForTimeoutAsync(500);
        }

        return null;
    }

    private static IEnumerable<IFrame> StripeFrames(IPage page)
        => page.Frames.Where(x => x.Url.Contains("stripe"));
}
