using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// Drives Stripe's embedded Checkout on the currently-loaded page (mounted in a <c>#checkout</c> div via
/// <c>data-stripe</c>/<c>data-stripe-checkout</c>). Embedded Checkout has no server-side "confirm" API, so
/// the card is entered in Stripe's iframes with the test card 4242 4242 4242 4242; on success Stripe
/// redirects the top frame off the checkout page. Completing the purchase itself is webhook-driven (poll the
/// DB) - this only gets the card accepted. Shared by the site- and chapter-subscription checkout flows,
/// which both mount checkout identically and differ only in the URL the caller navigates to first.
/// <para>
/// Stripe deeply nests (and lazily renders) its iframes: a payment-method accordion lives in the
/// <c>embedded-checkout-inner</c> frame, and once "Card" is selected the card fields render in that frame or
/// a nested one. So we select Card, then search every frame for the card-number field rather than assuming
/// where it lands. If Stripe changes things, the failure dumps every Stripe frame's inputs to show what to use.
/// </para>
/// </summary>
internal static class StripeEmbeddedCheckout
{
    private const string TestCardCvc = "123";
    private const string TestCardExpiry = "1234";
    private const string TestCardNumber = "4242424242424242";

    /// <summary>
    /// Pays the currently-loaded embedded Checkout with the Stripe test card, returning once Stripe has
    /// accepted the card and redirected the top frame off the checkout page. Throws with a per-frame
    /// diagnostic if the card form never appears.
    /// </summary>
    public static async Task PayWithTestCard(IPage page)
    {
        // Embedded Checkout can be taller than the default viewport (order summary + method accordion +
        // card form). A short viewport leaves lower controls "outside of the viewport" and unclickable even
        // after scrolling (they're inside Stripe's iframe), so use a tall viewport to keep them reachable.
        await page.SetViewportSizeAsync(1280, 2400);

        var checkout = await WaitForCheckoutFrame(page);

        // Card may already be the expanded method; look for its fields first (presence is the only reliable
        // expanded signal). If absent, click "Pay with card" to expand it - retrying, because Stripe
        // re-renders the accordion as the wallet/Link options finish loading and can collapse a just-opened
        // panel. Only clicked while the field is absent, so an already-open panel is never toggled shut.
        var cardFrame = await FindFrameWithCardField(page, 6);
        var expandOutcome = "(not attempted - card was already expanded)";
        for (var attempt = 0; attempt < 5 && cardFrame == null; attempt++)
        {
            expandOutcome = await ExpandCardAccordion(checkout);
            cardFrame = await FindFrameWithCardField(page, 8);
        }

        if (cardFrame == null)
        {
            throw new InvalidOperationException(await Diagnose(page, checkout) + " || Expand: " + expandOutcome);
        }

        await cardFrame.Locator("#billingName").FillAsync("E2E Test");

        // Set the country first (it drives whether a postal-code field renders). Only if the select exists.
        var country = cardFrame.Locator("#billingCountry");
        if (await country.CountAsync() > 0)
        {
            await country.SelectOptionAsync(new SelectOptionValue { Value = "GB" });
        }

        // Type char by char so Stripe's field formatters run (FillAsync sets the value directly).
        await cardFrame.Locator("#cardNumber").PressSequentiallyAsync(TestCardNumber);
        await cardFrame.Locator("#cardExpiry").PressSequentiallyAsync(TestCardExpiry);
        await cardFrame.Locator("#cardCvc").PressSequentiallyAsync(TestCardCvc);

        // A postal-code field only renders for some countries; fill it if present.
        var postalCode = cardFrame.Locator("#billingPostalCode");
        if (await postalCode.CountAsync() > 0)
        {
            await postalCode.PressSequentiallyAsync("SW1A 1AA");
        }

        await checkout.Locator("button[type=submit], .SubmitButton").First.ClickAsync();

        // On success Stripe redirects the top frame off the checkout page (to a confirm/return URL). Waiting
        // for the checkout page to be left confirms the card was accepted; the DB poll verifies completion.
        await page.WaitForURLAsync(url => !url.Contains("/checkout"), new() { Timeout = 30000 });
    }

    // Clicks the "Pay with card" accordion button to reveal the card form, returning a short outcome string
    // (surfaced in the diagnostic). The button can sit outside the viewport inside Stripe's iframe (a large
    // "expandedClickArea"), which blocks positional clicks even after scrolling; a JS click dispatches to the
    // element directly, bypassing viewport/actionability while still firing Stripe's React onClick handler.
    private static async Task<string> ExpandCardAccordion(IFrame checkout)
    {
        var cardButton = checkout.Locator("[data-testid='card-accordion-item-button']").First;
        if (await checkout.Locator("[data-testid='card-accordion-item-button']").CountAsync() == 0)
        {
            return "card button absent";
        }

        try
        {
            await cardButton.EvaluateAsync("el => el.click()");
            return "clicked (js)";
        }
        catch (PlaywrightException ex)
        {
            return $"click failed: {ex.Message}";
        }
    }

    // Lists the checkout frame's clickable controls (elements with a data-testid, plus buttons) with their
    // testid / aria-label / id, so the collapsed-state control that expands the card form is visible.
    private static async Task<string> DescribeClickables(IFrame checkout)
    {
        try
        {
            var items = await checkout.Locator("[data-testid], button").EvaluateAllAsync<string[]>(
                "els => els.map(function (e) { return e.tagName + " +
                "(e.getAttribute('data-testid') ? '[testid=' + e.getAttribute('data-testid') + ']' : '') + " +
                "(e.getAttribute('aria-label') ? '[aria=' + e.getAttribute('aria-label') + ']' : '') + " +
                "(e.id ? '#' + e.id : ''); })");
            return "[" + string.Join(", ", items) + "]";
        }
        catch (PlaywrightException ex)
        {
            return $"(could not read clickables: {ex.Message})";
        }
    }

    private static async Task<string> Diagnose(IPage page, IFrame checkout)
    {
        var parts = new List<string>();
        foreach (var frame in page.Frames)
        {
            if (!frame.Url.Contains("stripe"))
            {
                continue;
            }

            try
            {
                var inputs = await frame.Locator("input, select").EvaluateAllAsync<string[]>(
                    "els => els.map(function (e) { return '#' + (e.id || '') + '[name=' + (e.name || '') + ']'; })");
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

        var clickables = await DescribeClickables(checkout);

        return "Card field '#cardNumber' not found in any frame. Stripe frame inputs: " +
            string.Join(" || ", parts) + " || Checkout clickables: " + clickables;
    }

    private static async Task<IFrame?> FindFrameWithCardField(IPage page, int maxAttempts)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            foreach (var frame in page.Frames)
            {
                try
                {
                    if (await frame.Locator("#cardNumber").CountAsync() > 0)
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

    private static async Task<IFrame> WaitForCheckoutFrame(IPage page)
    {
        for (var attempt = 0; attempt < 60; attempt++)
        {
            var frame = page.Frames.FirstOrDefault(f => f.Url.Contains("embedded-checkout-inner"));
            if (frame != null)
            {
                return frame;
            }

            await page.WaitForTimeoutAsync(500);
        }

        // Embedded checkout never mounted. Usually the ClientSecret is empty because the server-side
        // checkout-session creation failed (e.g. Stripe rejected the connected-account transfer
        // destination), or the page rendered an error. Surface the #checkout secret + the page text.
        var checkoutDiv = page.Locator("#checkout");
        var clientSecret = await checkoutDiv.CountAsync() > 0
            ? await checkoutDiv.GetAttributeAsync("data-stripe-checkout") ?? "(attribute absent)"
            : "(no #checkout element)";
        var body = await page.InnerTextAsync("body");
        throw new InvalidOperationException(
            "Stripe embedded checkout did not mount. " +
            $"#checkout data-stripe-checkout='{clientSecret}' (empty => the server did not create a checkout " +
            $"session - check the app log for a Stripe error, likely the connected-account transfer destination). " +
            $"Body: {body[..Math.Min(600, body.Length)]}");
    }
}
