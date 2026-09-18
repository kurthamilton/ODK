namespace ODK.Core.Chapters;

public class ChapterQuestion : IDatabaseEntity, IChapterEntity
{
    public string AnswerHtml { get; set; } = string.Empty;

    public Guid ChapterId { get; set; }

    public int DisplayOrder { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
