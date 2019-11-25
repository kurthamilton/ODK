using System;

namespace ODK.Core.Countries
{
    public class Country
    {
        public Country(Guid id, string name, string currencyCode)
        {
            CurrencyCode = currencyCode;
            Id = id;
            Name = name;
        }

        public string CurrencyCode { get; }

        public Guid Id { get; }

        public string Name { get; }
    }
}
