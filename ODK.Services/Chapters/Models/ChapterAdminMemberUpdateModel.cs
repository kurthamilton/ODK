namespace ODK.Services.Chapters.Models;

public class ChapterAdminMemberUpdateModel
{
    public string? AdminEmailAddress { get; set; }

    public bool ReceiveContactEmails { get; set; }

    public bool ReceiveEventCommentEmails { get; set; }

    public bool ReceiveNewMemberEmails { get; set; }
}