using ODK.Core.Countries;

namespace ODK.Services.Chapters;

public class ChapterCreateModel
{
    public required string Description { get; set; }

    public required LatLong Location { get; set; }

    public required string LocationName { get; set; }

    public required string Name { get; set; }

    public required string? TimeZoneId { get; set; }
}
