using ODK.Core.Chapters;

namespace ODK.Data.Core.Chapters;

public class ChapterDto
{
    public required Chapter Chapter { get; init; }

    public required ChapterImageVersionDto? Image { get; init; }

    public required ChapterTexts? Texts { get; init; }
}