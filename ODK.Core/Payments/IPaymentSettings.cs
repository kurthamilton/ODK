namespace ODK.Core.Payments;

public interface IPaymentSettings
{
    string? ApiPublicKey { get; }

    string? ApiSecretKey { get; }

    bool HasApiKey { get; }

    PaymentProviderType? Provider { get; }
}
