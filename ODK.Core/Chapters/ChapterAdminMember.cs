using ODK.Core.Emails;
using ODK.Core.Members;

namespace ODK.Core.Chapters;

public class ChapterAdminMember : IDatabaseEntity
{
    public string? AdminEmailAddress { get; set; }

    public Guid ChapterId { get; set; }

    public Guid Id { get; set; }

    public Member Member { get; set; } = null!;

    public Guid MemberId { get; set; }

    public bool ReceiveContactEmails { get; set; }

    public bool ReceiveEventCommentEmails { get; set; }

    public bool ReceiveNewMemberEmails { get; set; }

    public ChapterAdminRole Role { get; set; }

    public bool HasAccessTo(ChapterAdminRole target)
    {
        return target switch
        {
            ChapterAdminRole.Owner =>
                Role == ChapterAdminRole.Owner,
            ChapterAdminRole.Admin =>
                Role == ChapterAdminRole.Owner ||
                Role == ChapterAdminRole.Admin,
            ChapterAdminRole.Organiser =>
                Role == ChapterAdminRole.Owner ||
                Role == ChapterAdminRole.Admin ||
                Role == ChapterAdminRole.Organiser,
            _ => false
        };
    }

    public EmailAddressee ToEmailAddressee()
    {
        var address = !string.IsNullOrEmpty(AdminEmailAddress)
            ? AdminEmailAddress
            : Member.EmailAddress;

        return new EmailAddressee(address, Member.FullName);
    }
}