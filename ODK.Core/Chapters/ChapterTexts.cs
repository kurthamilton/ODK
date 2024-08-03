namespace ODK.Core.Chapters;

public class ChapterTexts : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public string RegisterText { get; set; } = "";

    public string WelcomeText { get; set; } = "";
}
