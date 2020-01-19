using System;

namespace ODK.Web.Common.Countries.Responses
{
    public class CountryApiResponse
    {
        public string Continent { get; set; }

        public string CurrencyCode { get; set; }

        public string CurrencySymbol { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
