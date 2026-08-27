namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// Every Stripe payment settings record the browsing platform has, and the state of each one's webhook
/// endpoints. Scoped to the platform because its records are: each platform has its own Stripe accounts, and
/// the other platform's overview is on the other platform's site.
/// </summary>
public class SiteAdminStripeWebhooksViewModel
{
    public required IReadOnlyCollection<SiteAdminStripeWebhookAccountViewModel> Accounts { get; init; }

    public bool HasFindings => Accounts.Any(x => x.HasFindings);
}
