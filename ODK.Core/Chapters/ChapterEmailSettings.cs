namespace ODK.Core.Chapters;

public class ChapterEmailSettings : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public string FromEmailAddress { get; set; } = "";

    public string FromName { get; set; } = "";
}
