namespace ODK.Core.Subscriptions;

public static class SiteSubscriptionFrequencyExtensions
{
    public static string PeriodUnit(this SiteSubscriptionFrequency frequency) => frequency switch
    {
        SiteSubscriptionFrequency.Monthly => "month",
        SiteSubscriptionFrequency.Yearly => "year",
        _ => throw new NotSupportedException()
    };

    public static string PeriodUnitShort(this SiteSubscriptionFrequency frequency) => frequency switch
    {
        SiteSubscriptionFrequency.Monthly => "mo",
        SiteSubscriptionFrequency.Yearly => "yr",
        _ => throw new NotSupportedException()
    };
}