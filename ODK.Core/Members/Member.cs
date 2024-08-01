using ODK.Core.Emails;
using ODK.Core.Utils;

namespace ODK.Core.Members;

public class Member : IVersioned, IDatabaseEntity
{
    public bool Activated { get; set; }

    public ICollection<MemberChapter> Chapters { get; set; } = new List<MemberChapter>();

    public DateTime CreatedUtc { get; set; }

    public bool Disabled { get; set; }

    public string EmailAddress { get; set; } = "";

    public bool EmailOptIn { get; set; }

    public string FirstName { get; set; } = "";

    public string FullName => NameUtils.FullName(FirstName, LastName);

    public Guid Id { get; set; }

    public string LastName { get; set; } = "";

    public bool SuperAdmin { get; set; }

    public byte[] Version { get; set; } = [];

    public bool CanBeViewedBy(Member other) => IsCurrent() && SharesChapterWith(other);

    public EmailAddressee GetEmailAddressee() => new EmailAddressee(EmailAddress, FullName);    

    public bool IsMemberOf(Guid chapterId) => Chapters.Any(x => x.ChapterId == chapterId) || SuperAdmin;

    public bool IsCurrent() => Activated && !Disabled;

    public bool SharesChapterWith(Member other) => other.Chapters.Any(x => IsMemberOf(x.ChapterId));
}
