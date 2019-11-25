using ODK.Core.Members;

namespace ODK.Core.Payments
{
    public interface ISubscriptionPayment : IPayment
    {
        MemberTypes? SubscriptionType { get; }
    }
}
