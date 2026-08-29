using ODK.Core.Subscriptions;

namespace ODK.Services.Payments;

public class ExternalSubscriptionPlan
{
    public required decimal Amount { get; init; }
    public required string CurrencyCode { get; init; }
    public required string ExternalId { get; init; }
    public required string ExternalProductId { get; init; }
    public required SiteSubscriptionFrequency Frequency { get; init; }
    public required string Name { get; init; }
    public required int NumberOfMonths { get; init; }
    public required bool Recurring { get; init; }

    /// <summary>
    /// A single charge rather than a plan. Nothing recurs, so there is no plan on the provider to name and
    /// no frequency to state - the product it sits under is all the provider is given.
    /// </summary>
    public static ExternalSubscriptionPlan OneOff(
        decimal amount, string currencyCode, string externalProductId, string name) => new()
    {
        Amount = amount,
        CurrencyCode = currencyCode,
        ExternalId = string.Empty,
        ExternalProductId = externalProductId,
        Frequency = SiteSubscriptionFrequency.None,
        Name = name,
        NumberOfMonths = 0,
        Recurring = false
    };
}
