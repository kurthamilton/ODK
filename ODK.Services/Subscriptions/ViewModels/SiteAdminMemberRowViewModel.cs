using ODK.Core.Countries;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteAdminMemberRowViewModel
{
    public required decimal? Amount { get; init; }

    public required IReadOnlyCollection<string> ChapterNames { get; init; }

    public required Currency? Currency { get; init; }

    public required DateTime? ExpiresUtc { get; init; }

    public required SiteSubscriptionFrequency Frequency { get; init; }

    public required string FullName { get; init; }

    public required bool IsActive { get; init; }

    public required string SubscriptionName { get; init; }
}
