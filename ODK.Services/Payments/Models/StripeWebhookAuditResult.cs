namespace ODK.Services.Payments.Models;

/// <summary>
/// What the audit found about one payment settings record's Stripe account.
/// </summary>
public class StripeWebhookAuditResult
{
    /// <summary>
    /// Endpoints Stripe will not deliver to. Reported so they can be seen and tidied away, and checked
    /// against nothing: an endpoint somebody switched off is superseded rather than broken, so it counts
    /// towards neither a missing kind nor a duplicated one. Each states no checks and no event differences -
    /// only what it is.
    /// </summary>
    public required IReadOnlyCollection<StripeWebhookEndpointAudit> DisabledEndpoints { get; init; }

    /// <summary>
    /// A kind the account has more than one endpoint for. Both may pass every check while only one carries
    /// the signing secret the app holds, so a duplicate is a silent half-failure rather than redundancy.
    /// </summary>
    public required IReadOnlyCollection<StripeWebhookKind> DuplicateKinds { get; init; }

    /// <summary>
    /// Every endpoint Stripe will deliver to, ordered by kind with the unroutable ones last.
    /// </summary>
    public required IReadOnlyCollection<StripeWebhookEndpointAudit> Endpoints { get; init; }

    /// <summary>
    /// The record names no environment, so nothing that depends on knowing one could be compared - the host
    /// it should be on, and whether its account should be live.
    /// </summary>
    public required bool EnvironmentNotSet { get; init; }

    /// <summary>A kind the account has no endpoint for. Those events are not being delivered at all.</summary>
    public required IReadOnlyCollection<StripeWebhookKind> MissingKinds { get; init; }

    /// <summary>
    /// The account's endpoints do not all render events as the same Stripe API version, so the two are
    /// receiving different payload shapes. Informational - the app reads the fields it needs from either.
    /// </summary>
    public required bool MixedApiVersions { get; init; }
}
