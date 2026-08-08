using ODK.Core.Emails;
using ODK.Core.Utils;

namespace ODK.Core.Members;

public class Member : IVersioned, IDatabaseEntity, ITimeZoneEntity
{
    public const string DefaultTimeZoneId = "GMT Standard Time";

    public bool Activated { get; set; }

    public ICollection<MemberChapter> Chapters { get; set; } = [];

    public DateTime CreatedUtc { get; set; }

    public string EmailAddress { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string FullName => NameUtils.FullName(FirstName, LastName);

    public Guid Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The referral this member signed up from, when they arrived via a referral link. Null for every
    /// other member, which is the overwhelming majority.
    /// </summary>
    public Guid? ReferralId { get; set; }

    /// <summary>
    /// Whether the signup was flagged as likely automated, decided when the account was created against the
    /// score threshold in force at that moment. Stored rather than derived so that changing the threshold
    /// later doesn't retrospectively change who was flagged. Null when no check ran (the account predates
    /// the check, or reCAPTCHA was disabled). Flagging never blocks signup.
    /// </summary>
    public bool? RecaptchaFlagged { get; set; }

    /// <summary>
    /// reCAPTCHA score captured when the account was created, kept alongside <see cref="RecaptchaFlagged"/>
    /// as a record of how borderline the signup was. Null when no check ran.
    /// </summary>
    public double? RecaptchaScore { get; set; }

    public bool SiteAdmin { get; set; }

    public TimeZoneInfo TimeZone
    {
        get => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        set => TimeZoneId = value.Id;
    }

    public string TimeZoneId { get; set; } = DefaultTimeZoneId;

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

        return name;
    }

    public EmailAddressee ToEmailAddressee() => new EmailAddressee(EmailAddress, FullName);

    public MemberChapter? MemberChapter(Guid chapterId) => Chapters
        .FirstOrDefault(x => x.ChapterId == chapterId);

    public bool IsApprovedMemberOf(Guid chapterId) => MemberChapter(chapterId)?.Approved == true;

    public bool IsMemberOf(Guid chapterId) => MemberChapter(chapterId) != null;

    public bool IsCurrent() => Activated;

    public bool SharesChapterWith(Member other) => other.Chapters
        .Where(x => x.Approved)
        .Any(x => IsApprovedMemberOf(x.ChapterId));

    public bool Visible(Guid chapterId) => MemberChapter(chapterId)?.HideProfile == false;
}