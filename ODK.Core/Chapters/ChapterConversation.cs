namespace ODK.Core.Chapters;

public class ChapterConversation : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public string Subject { get; set; } = "";
}