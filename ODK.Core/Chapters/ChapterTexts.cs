namespace ODK.Core.Chapters;

public class ChapterTexts : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public string? Description { get; set; }

    public string RegisterText { get; set; } = string.Empty;

    public string WelcomeText { get; set; } = string.Empty;
}
