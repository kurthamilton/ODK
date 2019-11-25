namespace ODK.Payments
{
    public class PaymentDetails
    {
        public double Amount { get; set; }

        public string ApiSecretKey { get; set; }

        public string CurrencyCode { get; set; }

        public string EmailAddress { get; set; }
    }
}
