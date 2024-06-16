namespace ODK.Core.Chapters;

public class ChapterMembershipSettings
{
    public ChapterMembershipSettings(Guid chapterId, int trialPeriodMonths,
        int membershipDisabledAfterDaysExpired, bool enabled)
    {
        ChapterId = chapterId;
        Enabled = enabled;
        MembershipDisabledAfterDaysExpired = membershipDisabledAfterDaysExpired;
        TrialPeriodMonths = trialPeriodMonths;
    }

    public Guid ChapterId { get; }

    public bool Enabled { get; set; }

    public int MembershipDisabledAfterDaysExpired { get; set; }

    public int MembershipExpiringWarningDays => 30;

    public int TrialPeriodMonths { get; set; }
}
