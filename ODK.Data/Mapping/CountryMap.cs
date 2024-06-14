using System.Data;
using ODK.Core.Countries;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping;

public class CountryMap : SqlMap<Country>
{
    public CountryMap()
        : base("Countries")
    {
        Property(x => x.Id).HasColumnName("CountryId");
        Property(x => x.Name);
        Property(x => x.Continent);
        Property(x => x.CurrencyCode);
        Property(x => x.CurrencySymbol);
    }

    public override Country Read(IDataReader reader)
    {
        return new Country
        (
            id: reader.GetGuid(0),
            name: reader.GetString(1),
            continent: reader.GetString(2),
            currencyCode: reader.GetString(3),
            currencySymbol: reader.GetString(4)
        );
    }
}
