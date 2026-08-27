using ODK.Core.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// One payment settings record's Stripe account, and what its webhook endpoints look like.
/// </summary>
public class SiteAdminStripeWebhookAccountViewModel
{
    /// <summary>
    /// Endpoints Stripe will not deliver to. Shown so they can be found and removed, and checked against
    /// nothing - see <see cref="Models.StripeWebhookAuditResult.DisabledEndpoints"/>.
    /// </summary>
    public required IReadOnlyCollection<SiteAdminStripeWebhookViewModel> DisabledWebhooks { get; init; }

    public required IReadOnlyCollection<StripeWebhookKind> DuplicateKinds { get; init; }

    public required bool EnvironmentNotSet { get; init; }

    /// <summary>
    /// Why the account could not be read, where it could not be - a revoked key, a network failure. Set
    /// means nothing was compared, so nothing below is reported either way.
    /// </summary>
    public required string? Error { get; init; }

    public bool HasFindings
        => Error != null
            || DuplicateKinds.Count > 0
            || MissingKinds.Count > 0
            || Webhooks.Any(x => x.UnmetChecks.Count > 0);

    public required IReadOnlyCollection<StripeWebhookKind> MissingKinds { get; init; }

    public required bool MixedApiVersions { get; init; }

    public required SitePaymentSettings PaymentSettings { get; init; }

    public required IReadOnlyCollection<SiteAdminStripeWebhookViewModel> Webhooks { get; init; }
}
