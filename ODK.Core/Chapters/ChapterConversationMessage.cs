using ODK.Core.Members;

namespace ODK.Core.Chapters;

public class ChapterConversationMessage : IDatabaseEntity
{
    public Guid ChapterConversationId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Member? Member { get; set; }

    public Guid MemberId { get; set; }

    public double? RecaptchaScore { get; set; }

    public string Text { get; set; } = "";
}
