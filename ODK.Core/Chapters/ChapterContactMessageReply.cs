using ODK.Core.Members;

namespace ODK.Core.Chapters;

public class ChapterContactMessageReply : IDatabaseEntity
{
    public Guid ChapterContactMessageId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Member? Member { get; set; }

    public Guid MemberId { get; set; }

    public string Message { get; set; } = "";
}
