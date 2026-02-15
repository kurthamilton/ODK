using ODK.Core.Countries;

namespace ODK.Data.Core.Chapters;

public class ChapterSearchCriteriaDistance
{
    public required double DistanceKm { get; init; }

    public required LatLong Location { get; init; }
}