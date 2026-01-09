namespace ODK.Services.Payments;

public class ExternalSubscription
{
    public required DateTime? CancelDate { get; init; }

    public required string? ConnectedAccountId { get; init; }

    public required string ExternalId { get; init; }

    public required string ExternalSubscriptionPlanId { get; init; }

    public required IDictionary<string, string> Metadata { get; init; }

    public required DateTime? LastPaymentDate { get; init; }

    public required DateTime? NextBillingDate { get; init; }

    public required ExternalSubscriptionStatus Status { get; init; }
}