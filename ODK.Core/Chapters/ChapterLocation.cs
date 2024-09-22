using ODK.Core.Countries;

namespace ODK.Core.Chapters;

public class ChapterLocation : ILocation
{
    public Guid ChapterId { get; set; }

    public LatLong LatLong { get; set; }

    public string Name { get; set; } = "";

    public double DistanceFrom(ILocation other, DistanceUnit unit)
        => LatLong.DistanceFrom(other.LatLong, unit);
}
