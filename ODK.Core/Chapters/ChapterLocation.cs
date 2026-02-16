using ODK.Core.Countries;

namespace ODK.Core.Chapters;

public class ChapterLocation : IChapterEntity, ILocation
{
    public Guid ChapterId { get; set; }

    public double Latitude { get; set; }

    public LatLong LatLong => new LatLong(Latitude, Longitude);

    public double Longitude { get; set; }

    public string Name { get; set; } = string.Empty;
}