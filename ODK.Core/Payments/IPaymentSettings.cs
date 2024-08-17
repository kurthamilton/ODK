using ODK.Core.Countries;

namespace ODK.Core.Payments;

public interface IPaymentSettings
{
    string? ApiPublicKey { get; }

    string? ApiSecretKey { get; }

    Currency Currency { get; }

    Guid CurrencyId { get; }

    bool HasApiKey { get; }

    PaymentProviderType? Provider { get; }
}
