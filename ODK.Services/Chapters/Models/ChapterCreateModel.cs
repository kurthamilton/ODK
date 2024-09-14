using ODK.Core.Countries;

namespace ODK.Services.Chapters.Models;

public class ChapterCreateModel
{
    public required string Description { get; init; }

    public required byte[] ImageData { get; init; }

    public required LatLong Location { get; init; }

    public required string LocationName { get; init; }

    public required string Name { get; init; }

    public required string? TimeZoneId { get; init; }

    public required IReadOnlyCollection<Guid> TopicIds { get; init; }
}
