namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells a member their site subscription has lapsed.
/// </summary>
public sealed class SiteSubscriptionExpiredParameters : EmailTypeParameters
{
    private const string SubscriptionUrlName = "account.urls.siteSubscription";

    public static IReadOnlyCollection<string> Names { get; } = [SubscriptionUrlName];

    public required string SubscriptionUrl { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, SubscriptionUrlName, SubscriptionUrl);
    }
}
