using NetTopologySuite.Geometries;
using ODK.Core.Countries;

namespace ODK.Data.EntityFramework.Extensions;

internal static class DataTypeExtensions
{
    public static LatLong ToLatLong(this Point value) => new LatLong(value.Y, value.X);

    // NB x,y is long,lat - not lat,long
    // https://learn.microsoft.com/en-us/ef/core/modeling/spatial#longitude-and-latitude
    public static Point ToPoint(this LatLong value) => new Point(value.Long, value.Lat) { SRID = 4326 };
}
