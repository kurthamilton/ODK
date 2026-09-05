using ODK.Services.Payments.Models;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// A current subscription record of ours naming a Stripe subscription the account does not have.
/// </summary>
public class SiteAdminStripeUnaccountedSubscriptionViewModel
{
    /// <summary>The group it is for, where it is a group subscription.</summary>
    public required string? ChapterName { get; init; }

    public required DateTime? ExpiresUtc { get; init; }

    /// <summary>The Stripe subscription the record names, which is what could not be found.</summary>
    public required string ExternalId { get; init; }

    public required Guid Id { get; init; }

    public required string? MemberName { get; init; }

    public required StripeSubscriptionRecordType Type { get; init; }
}
