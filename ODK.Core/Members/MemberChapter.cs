namespace ODK.Core.Members;

public class MemberChapter : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
}
