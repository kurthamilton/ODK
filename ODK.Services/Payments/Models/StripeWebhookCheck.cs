namespace ODK.Services.Payments.Models;

/// <summary>
/// One comparison against a Stripe webhook endpoint, and how it came out.
/// </summary>
/// <remarks>
/// Carries the two values rather than a sentence about them: a caller renders them, and a test asserts on
/// them. <see cref="StripeWebhookCheckType.Events"/> states neither, because the lists that differ are on
/// <see cref="StripeWebhookEndpointAudit"/> where a reader can see which events they are.
/// </remarks>
public class StripeWebhookCheck
{
    public required string? Actual { get; init; }

    public required string? Expected { get; init; }

    public required StripeFindingSeverity Severity { get; init; }

    public required StripeWebhookCheckState State { get; init; }

    public required StripeWebhookCheckType Type { get; init; }
}
