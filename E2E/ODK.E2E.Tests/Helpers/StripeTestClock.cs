using ODK.E2E.Tests.Config;
using Stripe;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// Drives Stripe test clocks (Simulations) for recurring-subscription E2E tests. The app's Checkout creates
/// its customer via <c>CustomerEmail</c> (not on a clock), so a Checkout-created subscription can't be
/// advanced; instead this SDK-creates a customer on a fresh clock and a subscription on the app's recurring
/// price, tagged with the metadata the app's webhook processing needs. Advancing the clock past a billing
/// period makes Stripe emit a real renewal <c>invoice.payment_succeeded</c> webhook (delivered to the app
/// via the ngrok tunnel). Dispose deletes the clock, cascading its customer + subscription (no sandbox
/// litter). Uses the same Stripe secret the app is seeded with, so the app's price/account line up.
/// </summary>
internal sealed class StripeTestClock : IAsyncDisposable
{
    private readonly StripeClient _client;
    private readonly string _clockId;
    private readonly DateTime _frozenStart;
    private readonly string _subscriptionId;

    private StripeTestClock(StripeClient client, string clockId, DateTime frozenStart, string subscriptionId)
    {
        _client = client;
        _clockId = clockId;
        _frozenStart = frozenStart;
        _subscriptionId = subscriptionId;
    }

    /// <summary>
    /// Creates a test clock, a customer on it with a working default card, and a subscription on the given
    /// recurring Stripe price tagged with <paramref name="metadata"/> (the keys the app's webhook processing
    /// reads). The subscription charges synchronously, so its first invoice fires an initial
    /// invoice.payment_succeeded. Throws if the first invoice doesn't pay (no webhook would ever arrive).
    /// </summary>
    public static async Task<StripeTestClock> CreateSubscription(
        string apiSecretKey, string priceExternalId, IReadOnlyDictionary<string, string> metadata)
    {
        /* The key comes from the payment settings the app is transacting through rather than from test
           config: a clock, its customer and its subscription only exist inside one Stripe account, and the
           subscription has to be the one the app's webhook processing will be told about. */
        var client = new StripeClient(apiSecretKey);
        var frozenStart = DateTime.UtcNow;

        var clock = await new Stripe.TestHelpers.TestClockService(client).CreateAsync(
            new Stripe.TestHelpers.TestClockCreateOptions { FrozenTime = frozenStart });

        var customerService = new CustomerService(client);
        var customer = await customerService.CreateAsync(new CustomerCreateOptions { TestClock = clock.Id });

        // Give the customer a real default card so the subscription's invoices charge automatically. The
        // shared pm_card_visa token isn't reliably attachable, so build a PaymentMethod from the test card
        // token, attach it, and set it as the invoice default.
        var paymentMethods = new PaymentMethodService(client);
        var paymentMethod = await paymentMethods.CreateAsync(new PaymentMethodCreateOptions
        {
            Type = "card",
            Card = new PaymentMethodCardOptions { Token = "tok_visa" }
        });
        await paymentMethods.AttachAsync(
            paymentMethod.Id, new PaymentMethodAttachOptions { Customer = customer.Id });
        await customerService.UpdateAsync(customer.Id, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions { DefaultPaymentMethod = paymentMethod.Id }
        });

        var subscription = await new SubscriptionService(client).CreateAsync(new SubscriptionCreateOptions
        {
            Customer = customer.Id,
            Items = [new SubscriptionItemOptions { Price = priceExternalId }],
            Metadata = new Dictionary<string, string>(metadata),
            PaymentBehavior = "error_if_incomplete"
        });

        if (subscription.Status != "active")
        {
            throw new InvalidOperationException(
                $"Test-clock subscription not active after creation (status '{subscription.Status}'); the " +
                "first invoice did not pay, so no webhook will fire.");
        }

        return new StripeTestClock(client, clock.Id, frozenStart, subscription.Id);
    }

    /// <summary>
    /// The date Stripe will next charge the subscription, read from the same field the app reads
    /// (the subscription item's current period end). A subscription's stored expiry is expected to match it.
    /// </summary>
    public async Task<DateTime> GetNextPaymentDateUtc()
    {
        var subscription = await new SubscriptionService(_client).GetAsync(_subscriptionId);

        var item = subscription.Items?.Data?.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Test-clock subscription '{_subscriptionId}' has no items, so it has no billing period.");

        return item.CurrentPeriodEnd;
    }

    /// <summary>Advances the clock just past one billing month, triggering a renewal invoice + webhook.</summary>
    public async Task AdvanceOneMonth()
    {
        var service = new Stripe.TestHelpers.TestClockService(_client);
        await service.AdvanceAsync(_clockId, new Stripe.TestHelpers.TestClockAdvanceOptions
        {
            FrozenTime = _frozenStart.AddMonths(1).AddDays(1)
        });

        // Advancing is asynchronous on Stripe's side; wait for it to finish so the renewal webhook has been
        // emitted before the test polls.
        for (var attempt = 0; attempt < 60; attempt++)
        {
            var clock = await service.GetAsync(_clockId);
            if (clock.Status == "ready")
            {
                return;
            }

            await Task.Delay(1000);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await new Stripe.TestHelpers.TestClockService(_client).DeleteAsync(_clockId);
        }
        catch (StripeException)
        {
            // Best-effort cleanup; a leftover test clock in the sandbox is harmless.
        }
    }
}
