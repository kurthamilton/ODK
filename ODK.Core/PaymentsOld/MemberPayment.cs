using System;

namespace ODK.Core.Payments
{
    [Obsolete("TODO: Develop")]
    public class MemberPayment
    {
        public double Amount { get; set; }

        public string CurrencyCode { get; set; }

        public string CurrencyString => "";

        public DateTime Date { get; set; }
    }
}
