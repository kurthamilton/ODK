using System;

namespace ODK.Core.Payments
{
    public class MemberPayment
    {
        public double Amount { get; set; }

        public string CurrencyCode { get; set; }

        public string CurrencyString => PaymentsHelper.ToCurrencyString(CurrencyCode, Amount);

        public DateTime Date { get; set; }
    }
}
