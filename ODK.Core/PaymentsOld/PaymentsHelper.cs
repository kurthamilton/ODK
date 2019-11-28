namespace ODK.Core.Payments
{
    public static class PaymentsHelper
    {
        public static string GetCurrencySymbol(string currencyCode)
        {
            switch (currencyCode)
            {
                case "AUD":
                case "USD":
                    return "$";
                case "EUR":
                    return "€";
                case "GBP":
                    return "£";
            }

            return null;
        }

        public static string ToCurrencyString(string currencyCode, double amount)
        {
            string currencySymbol = GetCurrencySymbol(currencyCode);
            return $"{currencySymbol}{amount:0.00}";
        }
    }
}
