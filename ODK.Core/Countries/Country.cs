using System;

namespace ODK.Core.Countries
{
    public class Country
    {
        public Country(Guid id, string name, string currencyCode, string currencySymbol)
        {
            CurrencyCode = currencyCode;
            CurrencySymbol = currencySymbol;
            Id = id;
            Name = name;
        }

        public string CurrencyCode { get; }

        public string CurrencySymbol { get; }

        public Guid Id { get; }

        public string Name { get; }
    }
}
