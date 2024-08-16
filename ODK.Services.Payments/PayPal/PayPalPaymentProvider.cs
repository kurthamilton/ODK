using ODK.Core.Chapters;
using ODK.Services.Payments.PayPal.Client;

namespace ODK.Services.Payments.PayPal;

public class PayPalPaymentProvider : IPayPalPaymentProvider
{
    private readonly PayPalPaymentProviderSettings _settings;

    public PayPalPaymentProvider(PayPalPaymentProviderSettings settings)
    {
        _settings = settings;
    }

    public bool HasExternalGateway => false;

    public async Task<ServiceResult> MakePayment(ChapterPaymentSettings paymentSettings, string currencyCode, decimal amount, 
        string cardToken, string description, string memberName)
    {
        var client = GetClient(paymentSettings);
        if (client == null)
        {
            return ServiceResult.Failure("Unable to load payment");
        }

        OrderJsonModel? order = await client.GetOrderAsync(cardToken);
        if (order == null || order.PurchaseUnits.Length != 1)
        {
            return ServiceResult.Failure("Payment not found in PayPal");
        }

        PurchaseUnitJsonModel purchase = order.PurchaseUnits[0];

        bool approved = string.Equals(purchase.Amount?.CurrencyCode, currencyCode, StringComparison.InvariantCultureIgnoreCase) &&
            purchase.Amount?.Value == amount &&
            string.Equals("APPROVED", order.Status, StringComparison.InvariantCultureIgnoreCase);
        if (!approved)
        {
            return ServiceResult.Failure($"Payment not approved in PayPal. Current status: {order.Status}");
        }

        OrderCaptureJsonModel? capture = await client.CaptureOrderPaymentAsync(order.Id);
        if (!string.Equals("COMPLETED", capture?.Status, StringComparison.InvariantCultureIgnoreCase))
        {
            return ServiceResult.Failure($"Payment not completed in PayPal. Current status: {order.Status}");
        }

        return ServiceResult.Successful();
    }

    public Task<ServiceResult> VerifyPayment(ChapterPaymentSettings paymentSettings, string currencyCode, decimal amount, string cardToken)
    {
        throw new NotImplementedException();
    }

    private PayPalClient? GetClient(ChapterPaymentSettings paymentSettings)
    {
        if (paymentSettings.ApiPublicKey == null || paymentSettings.ApiSecretKey == null)
        {
            return null;
        }

        return new PayPalClient(_settings.ApiBaseUrl, paymentSettings.ApiPublicKey, paymentSettings.ApiSecretKey);
    }
}
