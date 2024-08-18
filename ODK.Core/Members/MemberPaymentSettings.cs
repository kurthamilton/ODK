using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Core.Members;

public class MemberPaymentSettings : IPaymentSettings
{
    public string? ApiPublicKey { get; set; }

    public string? ApiSecretKey { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public bool HasApiKey => !string.IsNullOrEmpty(ApiPublicKey) && !string.IsNullOrEmpty(ApiSecretKey);

    public Guid MemberId { get; set; }

    public PaymentProviderType? Provider { get; set; }
}
