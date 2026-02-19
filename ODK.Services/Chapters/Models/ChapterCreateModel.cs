using ODK.Core.Countries;
using ODK.Services.Topics.Models;

namespace ODK.Services.Chapters.Models;

public class ChapterCreateModel
{
    public required string Description { get; init; }

    public required byte[] ImageData { get; init; }

    public required LatLong Location { get; init; }

    public required string LocationName { get; init; }

    public required string Name { get; init; }

    public required IReadOnlyCollection<NewTopicModel> NewTopics { get; init; }

    public required string ShortDescription { get; init; }

    public required IReadOnlyCollection<Guid> TopicIds { get; init; }
}