namespace ODK.Services.Payments;

public class ExternalSubscription
{
    public required string ExternalId { get; init; }

    public required string ExternalSubscriptionPlanId { get; init; }

    public required bool IsActive { get; init; }

    public required DateTime? LastPaymentDate { get; init; }

    public required DateTime? NextBillingDate { get; init; }
}
