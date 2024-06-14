namespace ODK.Services.Payments.PayPal
{
    public class PayPalPaymentProviderSettings
    {
        public PayPalPaymentProviderSettings(string apiBaseUrl)
        {
            ApiBaseUrl = apiBaseUrl;
        }

        public string ApiBaseUrl { get; }
    }
}
