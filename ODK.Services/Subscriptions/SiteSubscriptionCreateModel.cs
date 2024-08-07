namespace ODK.Services.Subscriptions;

public class SiteSubscriptionCreateModel
{
    public double Amount { get; set; }

    public string Description { get; set; } = "";

    public bool Enabled { get; set; }

    public int? GroupLimit { get; set; }

    public int? MemberLimit { get; set; }

    public bool MemberSubscriptions { get; set; }

    public string Name { get; set; } = "";

    public bool Premium { get; set; }

    public List<SiteSubscriptionPriceCreateModel> Prices { get; set; } = new();

    public bool SendMemberEmails { get; set; }
}
