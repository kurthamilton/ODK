namespace ODK.Services.Chapters.Models;

public class UpdateChapterAdminMember
{
    public string? AdminEmailAddress { get; set; }

    public bool ReceiveContactEmails { get; set; }

    public bool ReceiveEventCommentEmails { get; set; }

    public bool ReceiveNewMemberEmails { get; set; }
}