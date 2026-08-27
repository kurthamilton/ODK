namespace ODK.Services.Payments.Models;

/// <summary>
/// A webhook endpoint as Stripe reports it. Carries no signing secret: Stripe returns one on the create
/// response only, so nothing that reads an existing endpoint can see it.
/// </summary>
public class StripeWebhookEndpoint
{
    /// <summary>The Stripe API version events are rendered as for this endpoint.</summary>
    public required string? ApiVersion { get; init; }

    public required string? Description { get; init; }

    public required bool Enabled { get; init; }

    public required IReadOnlyCollection<string> Events { get; init; }

    public required string Id { get; init; }

    /// <summary>
    /// Whether the endpoint belongs to a live-mode account rather than a test one. A record's environment
    /// implies which this should be, so the two disagreeing means the record holds the wrong keys.
    /// </summary>
    public required bool LiveMode { get; init; }

    public required string Url { get; init; }
}
