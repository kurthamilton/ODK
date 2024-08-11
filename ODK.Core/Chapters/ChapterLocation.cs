using ODK.Core.Countries;

namespace ODK.Core.Chapters;

public class ChapterLocation
{
    public Guid ChapterId { get; set; }

    public LatLong LatLong { get; set; }

    public string Name { get; set; } = "";
}
