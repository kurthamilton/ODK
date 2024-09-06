using ODK.Core.Members;

namespace ODK.Core.Chapters;

public class ChapterConversationMessage : IDatabaseEntity
{
    public Guid ChapterConversationId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Member? Member { get; set; }

    public Guid MemberId { get; set; }

    public bool ReadByChapter { get; set; }

    public bool ReadByMember { get; set; }

    public double? RecaptchaScore { get; set; }

    public string Text { get; set; } = "";

    public ChapterConversationMessage Clone() => new ChapterConversationMessage
    {
        ChapterConversationId = ChapterConversationId,
        CreatedUtc = CreatedUtc,
        Id = Id,
        Member = Member,
        MemberId = MemberId,
        ReadByChapter = ReadByChapter,
        ReadByMember = ReadByMember,
        RecaptchaScore = RecaptchaScore,
        Text = Text
    };
}
