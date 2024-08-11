namespace ODK.Core.Chapters;

public class ChapterWithLocationDto
{
    public required Chapter Chapter { get; init; }

    public required ChapterLocation Location { get; init; }
}
