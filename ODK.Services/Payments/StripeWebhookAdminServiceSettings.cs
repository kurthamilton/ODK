using ODK.Core.Platforms;

namespace ODK.Services.Payments;

/// <summary>
/// What a Stripe webhook endpoint is expected to look like, and how to address one in Stripe's dashboard.
/// </summary>
/// <remarks>
/// Every value here can legitimately be unstated, and unstated means <em>not comparable</em> - a check that
/// cannot be made, reported as such and never as met or unmet. See <see cref="StripeWebhookAudit"/>.
/// </remarks>
public class StripeWebhookAdminServiceSettings
{
    public required IReadOnlyCollection<string> Events { get; init; }

    public required IReadOnlyDictionary<EnvironmentType, IReadOnlyDictionary<PlatformType, string>> Hosts { get; init; }

    public required string LiveDashboardUrlFormat { get; init; }

    public required string Path { get; init; }

    public required string TestDashboardUrlFormat { get; init; }
}
