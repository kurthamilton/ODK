using ODK.Core.Features;
using ODK.Core.Platforms;

namespace ODK.Core.Subscriptions;

public class SiteSubscription : IDatabaseEntity
{
    public bool Default { get; set; }

    public string Description { get; set; } = "";

    public bool Enabled { get; set; }

    public string? ExternalProductId { get; set; }

    public Guid? FallbackSiteSubscriptionId { get; set; }

    public int? GroupLimit { get; set; }

    public Guid Id { get; set; }

    public int? MemberLimit { get; set; }

    public bool MemberSubscriptions { get; set; }

    public string Name { get; set; } = "";

    public PlatformType Platform { get; set; }

    public bool Premium { get; set; }

    public bool SendMemberEmails { get; set; }

    public bool HasCapacity(int memberCount) => MemberLimit == null || memberCount < MemberLimit;

    public IEnumerable<SiteFeatureType> Features()
    {
        if (MemberSubscriptions)
        {
            yield return SiteFeatureType.MemberSubscriptions;
        }

        if (Premium)
        {
            yield return SiteFeatureType.EventTickets;
            yield return SiteFeatureType.InstagramFeed;
        }

        if (SendMemberEmails)
        {
            yield return SiteFeatureType.SendMemberEmails;
        }
    }

    public bool HasFeature(SiteFeatureType feature) => Features().Contains(feature);
}
