namespace ODK.Services.Chapters;

public class UpdateChapterMembershipSettings
{
    public bool Enabled { get; set; }

    public int MembershipDisabledAfterDaysExpired { get; set; }

    public int TrialPeriodMonths { get; set; }
}
