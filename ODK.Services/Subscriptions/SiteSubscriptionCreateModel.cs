namespace ODK.Services.Subscriptions;

public class SiteSubscriptionCreateModel
{
    public string Description { get; set; } = "";

    public bool Enabled { get; set; }

    public Guid? FallbackSiteSubscriptionId { get; set; }

    public int? GroupLimit { get; set; }

    public int? MemberLimit { get; set; }

    public bool MemberSubscriptions { get; set; }

    public string Name { get; set; } = "";

    public bool Premium { get; set; }

    public bool SendMemberEmails { get; set; }
}
