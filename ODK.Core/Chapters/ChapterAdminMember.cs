namespace ODK.Core.Chapters;

public class ChapterAdminMember
{    
    public string? AdminEmailAddress { get; set; }

    public Guid ChapterId { get; set; }

    public Guid MemberId { get; set; }

    public bool ReceiveContactEmails { get; set; }

    public bool ReceiveEventCommentEmails { get; set; }

    public bool ReceiveNewMemberEmails { get; set; }

    public bool SendNewMemberEmails { get; set; }
}
