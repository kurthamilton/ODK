namespace ODK.Core.Members;

[Flags]
public enum SubscriptionStatus
{
    None = 0,
    Current = 1,
    Expiring = 2,
    Expired = 4
}
