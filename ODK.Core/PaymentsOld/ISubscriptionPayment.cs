using System;
using ODK.Core.Members;

namespace ODK.Core.Payments
{
    [Obsolete("TODO: Remove")]
    public interface ISubscriptionPayment : IPayment
    {
        MemberTypes? SubscriptionType { get; }
    }
}
