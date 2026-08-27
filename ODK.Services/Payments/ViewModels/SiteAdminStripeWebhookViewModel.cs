using ODK.Services.Payments.Models;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// One Stripe webhook endpoint as the site admin overview shows it.
/// </summary>
public class SiteAdminStripeWebhookViewModel
{
    public required string? ApiVersion { get; init; }

    public required IReadOnlyCollection<StripeWebhookCheck> Checks { get; init; }

    /// <summary>
    /// Where to see this endpoint in Stripe, or null where the record names no account for a link to
    /// address. Not guessed: an unreachable link reads as a broken page rather than as missing data.
    /// </summary>
    public required string? DashboardUrl { get; init; }

    public required IReadOnlyCollection<string> Events { get; init; }

    public required IReadOnlyCollection<string> ExtraEvents { get; init; }

    public required string Id { get; init; }

    public required StripeWebhookKind Kind { get; init; }

    public required IReadOnlyCollection<string> MissingEvents { get; init; }

    public IReadOnlyCollection<StripeWebhookCheck> UnmetChecks
        => [.. Checks.Where(x => x.State == StripeWebhookCheckState.Unmet)];

    public required string Url { get; init; }
}
