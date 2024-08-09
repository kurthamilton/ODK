using ODK.Core.Platforms;

namespace ODK.Core.Subscriptions;

public class SiteSubscription : IDatabaseEntity
{
    public bool Default { get; set; }

    public string Description { get; set; } = "";

    public bool Enabled { get; set; }

    public int? GroupLimit { get; set; }

    public Guid Id { get; set; }

    public int? MemberLimit { get; set; }

    public bool MemberSubscriptions { get; set; }

    public string Name { get; set; } = "";

    public PlatformType? Platform { get; set; }

    public bool Premium { get; set; }

    public bool SendMemberEmails { get; set; }
}
