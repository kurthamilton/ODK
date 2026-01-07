using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using ODK.Core.Countries;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Converters;

internal class LatLongConverter : ValueConverter<LatLong, Point>
{
    public LatLongConverter()
        : base(
            x => x.ToPoint(),
            x => x.ToLatLong())
    {
    }
}
