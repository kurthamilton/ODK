namespace ODK.Core.Chapters;

public class ChapterTexts : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public string? DescriptionHtml { get; set; }

    public string RegisterTextHtml { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string WelcomeTextHtml { get; set; } = string.Empty;
}