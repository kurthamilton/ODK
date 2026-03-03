namespace ODK.Core.Chapters;

public class ChapterConversation : IDatabaseEntity, IChapterEntity, IMemberEntity
{
    public DateTime? ArchivedUtc { get; set; }

    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public required string Subject { get; set; }
}