namespace ODK.Core.Subscriptions;

public static class SiteSubscriptionFrequencyExtensions
{
    public static string PeriodUnit(this SiteSubscriptionFrequency frequency) => frequency switch
    {
        SiteSubscriptionFrequency.Monthly => "month",
        SiteSubscriptionFrequency.Yearly => "year",
        _ => throw new NotSupportedException()
    };
}
