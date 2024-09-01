namespace ODK.Core.Features;

public static class SiteFeatureTypeExtensions
{
    public static bool RequiresPayments(this SiteFeatureType feature)
    {
        switch (feature)
        {
            case SiteFeatureType.EventTickets:
            case SiteFeatureType.MemberSubscriptions:
                return true;
            default:
                return false;
        }
    }
}
