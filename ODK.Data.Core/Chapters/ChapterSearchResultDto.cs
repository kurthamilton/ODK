using ODK.Core.Chapters;
using ODK.Core.Topics;

namespace ODK.Data.Core.Chapters;

public class ChapterSearchResultDto
{
    public required Chapter Chapter { get; init; }

    public required ChapterImageMetadataDto? Image { get; init; }

    public required ChapterLocation? Location { get; init; }

    public required ChapterTexts? Texts { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}