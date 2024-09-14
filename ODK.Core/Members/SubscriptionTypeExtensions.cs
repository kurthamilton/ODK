namespace ODK.Core.Members;

public static class SubscriptionTypeExtensions
{
    public static bool IsPaid(this SubscriptionType type)
    {
        switch (type)
        {
            case SubscriptionType.Full:
            case SubscriptionType.Partial:
                return true;
            default:
                return false;
        }
    }
}
