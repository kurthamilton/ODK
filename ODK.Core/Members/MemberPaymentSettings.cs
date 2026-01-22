using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Core.Members;

public class MemberPaymentSettings
{
    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public Guid MemberId { get; set; }

    public PaymentProviderType? Provider { get; set; }
}