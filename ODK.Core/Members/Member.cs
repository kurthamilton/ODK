using ODK.Core.Emails;
using ODK.Core.Utils;

namespace ODK.Core.Members;

public class Member : IVersioned, IDatabaseEntity
{
    public bool Activated { get; set; }

    public Guid ChapterId { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool Disabled { get; set; }

    public string EmailAddress { get; set; } = "";

    public bool EmailOptIn { get; set; }

    public string FirstName { get; set; } = "";

    public string FullName => NameUtils.FullName(FirstName, LastName);

    public Guid Id { get; set; }

    public string LastName { get; set; } = "";

    public bool SuperAdmin { get; set; }

    public byte[] Version { get; set; } = [];

    public bool CanBeViewedBy(Member currentMember)
    {
        return currentMember?.ChapterId == ChapterId;
    }

    public EmailAddressee GetEmailAddressee()
    {
        return new EmailAddressee(EmailAddress, FullName);
    }

    public bool IsCurrent() => Activated && !Disabled;
}
