using ODK.Core.Emails;
using ODK.Core.Members;

namespace ODK.Core.Chapters;

public class ChapterAdminMember : IDatabaseEntity, IChapterEntity
{    
    public string? AdminEmailAddress { get; set; }

    public Guid ChapterId { get; set; }

    public Guid Id { get; set; }

    public Member Member { get; set; } = null!;

    public Guid MemberId { get; set; }

    public bool ReceiveContactEmails { get; set; }

    public bool ReceiveEventCommentEmails { get; set; }

    public bool ReceiveNewMemberEmails { get; set; }

    public bool SendNewMemberEmails { get; set; }

    public EmailAddressee ToEmailAddressee()
    {
        var address = !string.IsNullOrEmpty(AdminEmailAddress)
            ? AdminEmailAddress
            : Member.EmailAddress;

        return new EmailAddressee(address, Member.FullName);
    }
}
