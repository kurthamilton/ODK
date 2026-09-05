namespace ODK.Infrastructure.Settings;

/// <summary>
/// Where to see one of Stripe's own objects in its dashboard, with <c>{account}</c> and <c>{id}</c> to fill
/// in. A live-mode format and a test-mode one for each, because the two dashboards are different addresses
/// and every environment but prod transacts through a sandbox.
/// </summary>
/// <remarks>
/// An empty format means no link rather than a guessed one: a site admin following a link into the wrong
/// account's dashboard is worse served than one shown an id to search for.
/// </remarks>
public class StripeDashboardSettings
{
    public required string LiveInvoiceUrlFormat { get; init; }

    public required string LivePaymentUrlFormat { get; init; }

    public required string LiveSubscriptionUrlFormat { get; init; }

    public required string TestInvoiceUrlFormat { get; init; }

    public required string TestPaymentUrlFormat { get; init; }

    public required string TestSubscriptionUrlFormat { get; init; }
}
