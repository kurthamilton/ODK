using System;
using System.Collections.Generic;
using System.Linq;

namespace ODK.Core.Payments
{
    // TODO merge payment classes
    public class Payment
    {
        public Payment(Guid id, string identifier, int? memberId, string currencyCode, DateTime date, IEnumerable<PaymentDetail> details)
        {
            CurrencyCode = currencyCode;
            Date = date;
            Details = details.ToArray();
            Id = id;
            Identifier = identifier;
            MemberId = memberId;
        }

        public string CurrencyCode { get; }

        public DateTime Date { get; }

        public IReadOnlyCollection<PaymentDetail> Details { get; }

        public Guid Id { get; }

        public string Identifier { get; }

        public int? MemberId { get; }

        public double Total => Details.Sum(x => x.Amount);
    }
}
