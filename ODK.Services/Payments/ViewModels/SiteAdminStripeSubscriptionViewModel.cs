using ODK.Services.Payments.Models;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// One Stripe subscription as the overview shows it: what it carries, what it should carry, and who both
/// of those say it is for.
/// </summary>
public class SiteAdminStripeSubscriptionViewModel
{
    /// <summary>The group it bills for, where a group subscription's record or metadata names one.</summary>
    public required string? ChapterName { get; init; }

    public required DateTime CreatedUtc { get; init; }

    /// <inheritdoc cref="SiteAdminStripeWebhookViewModel.DashboardUrl"/>
    public required string? DashboardUrl { get; init; }

    /// <summary>
    /// The metadata the subscription should carry, worked out from the record it matched, for copying into
    /// Stripe key by key. Null where nothing matched it.
    /// </summary>
    public required IReadOnlyDictionary<string, string>? ExpectedMetadata { get; init; }

    public required IReadOnlyCollection<StripeTransactionFinding> Findings { get; init; }

    public required string Id { get; init; }

    /// <summary>The member it bills, from the metadata where that names one and from the record otherwise.</summary>
    public required string? MemberName { get; init; }

    /// <summary>What the subscription carries now, which is what its next invoice will carry.</summary>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }

    public required StripeSubscriptionStatus Status { get; init; }

    public bool HasFindings => Findings.Count > 0;
}
