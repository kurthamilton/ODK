namespace ODK.Core.Chapters;

public class ChapterMembershipSettings : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public bool Enabled { get; set; }

    public int MembershipDisabledAfterDaysExpired { get; set; }

    public int MembershipExpiringWarningDays { get; set; }

    public int TrialPeriodMonths { get; set; }
}
