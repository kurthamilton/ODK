using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using ODK.Core.Countries;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Converters;

internal class NullableLatLongConverter : ValueConverter<LatLong?, Point?>
{
    public NullableLatLongConverter()
        : base(
            // NB x,y is long,lat - not lat,long
            // https://learn.microsoft.com/en-us/ef/core/modeling/spatial#longitude-and-latitude
            x => x != null ? x.Value.ToPoint() : null,
            x => x != null ? x.ToLatLong() : null)
    {
    }
}
