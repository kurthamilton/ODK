using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

public class PaymentsStripeWebhooksSettings
{
    /// <summary>
    /// The events every webhook endpoint, on every account, is expected to be subscribed to. One list rather
    /// than one per endpoint: the endpoints are meant to be identical, and a list each is a list each to
    /// forget an event in. Empty means there is nothing to compare against, not that no events are wanted.
    /// </summary>
    public required string[] Events { get; init; }

    /// <summary>
    /// The host a record's endpoints are expected to be on, keyed by the record's environment and then its
    /// platform. An entry that is absent or empty leaves the host uncompared - the dev and e2e host is a
    /// tunnel to a developer's machine, so <c>appsettings.json</c> states it empty and each environment
    /// supplies its own.
    /// </summary>
    /// <remarks>
    /// Nullable at both levels because that is what the binder can deliver: <c>{}</c> produces no config keys
    /// at all, so an omitted section arrives as null however the property is declared. Declaring it so keeps
    /// the <c>?? []</c> in <c>DependencyRegistrar</c> compiler-enforced rather than remembered.
    /// </remarks>
    public required Dictionary<EnvironmentType, Dictionary<PlatformType, string>?>? Hosts { get; init; }

    /// <summary>
    /// The Stripe dashboard address of one webhook endpoint on a live-mode account, with <c>{account}</c> and
    /// <c>{id}</c> to fill in. Paired with <see cref="TestDashboardUrlFormat"/>, which the test-mode
    /// accounts - the sandboxes dev and e2e transact through - need instead.
    /// </summary>
    public required string LiveDashboardUrlFormat { get; init; }

    /// <summary>The path every webhook endpoint's URL is expected to address.</summary>
    public required string Path { get; init; }

    /// <inheritdoc cref="LiveDashboardUrlFormat"/>
    public required string TestDashboardUrlFormat { get; init; }
}
