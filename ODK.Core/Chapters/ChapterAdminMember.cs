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
        => Role.HasAccessTo(target, Member);

    public EmailAddressee ToEmailAddressee()
    {
        var address = !string.IsNullOrEmpty(AdminEmailAddress)
            ? AdminEmailAddress
            : Member.EmailAddress;

        return new EmailAddressee(address, Member.FullName);
    }
}