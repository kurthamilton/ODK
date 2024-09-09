using ODK.Core.Countries;

namespace ODK.Services.Chapters;

public class GroupFilter
{
    public required double? Distance { get; init; }

    public required string? DistanceUnit { get; init; }

    public required LatLong? Location { get; init; }

    public required string? LocationName { get; init; }

    public required string? TopicGroup { get; init; }
}
