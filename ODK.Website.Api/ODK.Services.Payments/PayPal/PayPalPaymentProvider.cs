using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Services.Payments.PayPal.Client;

namespace ODK.Services.Payments.PayPal
{
    public class PayPalPaymentProvider : IPayPalPaymentProvider
    {
        private readonly PayPalPaymentProviderSettings _settings;

        public PayPalPaymentProvider(PayPalPaymentProviderSettings settings)
        {
            _settings = settings;
        }

        public bool HasExternalGateway => false;

        public async Task<bool> MakePayment(ChapterPaymentSettings paymentSettings, string currencyCode, double amount, string cardToken, string description, string memberName)
        {
            PayPalClient client = GetClient(paymentSettings);

            OrderJsonModel order = await client.GetOrderAsync(cardToken);
            if (order == null || order.PurchaseUnits.Length != 1)
            {
                return false;
            }

            PurchaseUnitJsonModel purchase = order.PurchaseUnits[0];

            bool verified = string.Equals(purchase.Amount.CurrencyCode, currencyCode, StringComparison.InvariantCultureIgnoreCase) &&
                purchase.Amount.Value == amount &&
                string.Equals("APPROVED", order.Status, StringComparison.InvariantCultureIgnoreCase);
            if (!verified)
            {
                return false;
            }

            OrderCaptureJsonModel capture = await client.CaptureOrderPaymentAsync(order.Id);
            if (!string.Equals("COMPLETED", capture.Status, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public Task<bool> VerifyPayment(ChapterPaymentSettings paymentSettings, string currencyCode, double amount, string cardToken)
        {
            throw new NotImplementedException();
        }

        private PayPalClient GetClient(ChapterPaymentSettings paymentSettings)
        {
            return new PayPalClient(_settings.ApiBaseUrl, paymentSettings.ApiPublicKey, paymentSettings.ApiSecretKey);
        }
    }
}
