using ODK.Core.Countries;

namespace ODK.Data.Core.Chapters;

public class ChapterLocationDto : ILocation
{
    public required Guid ChapterId { get; init; }

    public required decimal Latitude { get; init; }

    public LatLong LatLong => new LatLong((double)Latitude, (double)Longitude);

    public required decimal Longitude { get; init; }

    public required string Name { get; init; }

    public double DistanceFrom(ILocation other, DistanceUnit unit)
        => LatLong.DistanceFrom(other.LatLong, unit);
}