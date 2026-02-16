using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using ODK.Core.Chapters;
using ODK.Core.Countries;

namespace ODK.Data.EntityFramework.Queries;

internal static class ChapterLocationQueries
{
    private const int WGS84Srid = 4326;

    internal static IQueryable<ChapterLocation> WithinDistance(
        this IQueryable<ChapterLocation> query,
        LatLong location,
        double radiusMetres)
    {
        var gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: WGS84Srid);
        var origin = gf.CreatePoint(new Coordinate(location.Long, location.Lat));

        return query
            .Where(x => EF.Property<Point>(x, "LatLongPoint").Distance(origin) <= radiusMetres);
    }
}