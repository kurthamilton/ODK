using ODK.Core.Platforms;

namespace ODK.Services.Payments.Models;

/// <summary>
/// One Stripe account, as the webhook audit needs to see it: which account to address in the dashboard,
/// and the deployment and platform its endpoints are expected to serve.
/// </summary>
public class StripePaymentAccount
{
    /// <summary>The provider's own id for the account, as it appears in the dashboard.</summary>
    public required string AccountId { get; init; }

    public required EnvironmentType Environment { get; init; }

    public required PlatformType Platform { get; init; }
}
