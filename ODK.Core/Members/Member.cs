using ODK.Core.Emails;
using ODK.Core.Utils;

namespace ODK.Core.Members;

public class Member : IVersioned, IDatabaseEntity, ITimeZoneEntity
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

    public ICollection<MemberChapterPrivacySettings> PrivacySettings { get; set; } = new HashSet<MemberChapterPrivacySettings>();

    public bool SuperAdmin { get; set; }

    public TimeZoneInfo? TimeZone { get; set; }

    public byte[] Version { get; set; } = [];

    public bool CanBeViewedBy(Member other) => IsCurrent() && SharesChapterWith(other);

    public string GetDisplayName(Guid chapterId)
    {
        OdkAssertions.MemberOf(this, chapterId);

        var visible = Visible(chapterId);

        var name = FullName;
        if (!visible)
        {
            name += " [HIDDEN]";
        }

        if (Disabled)
        {
            name += " [DISABLED]";
        }

        return name;
    }

    public EmailAddressee GetEmailAddressee() => new EmailAddressee(EmailAddress, FullName);    

    public MemberChapter MemberChapter(Guid chapterId) => Chapters.First(x => x.ChapterId == chapterId);

    public bool IsMemberOf(Guid chapterId) => Chapters.Any(x => x.ChapterId == chapterId);

    public bool IsCurrent() => Activated && !Disabled;

    public bool SharesChapterWith(Member other) => other.Chapters.Any(x => IsMemberOf(x.ChapterId));

    public bool Visible(Guid chapterId) => !PrivacySettings.Any(x => x.ChapterId == chapterId && x.HideProfile);
}
