namespace ODK.Services.Chapters.Models;

public class ChapterMembershipSettingsUpdateModel
{
    public required bool ApproveNewMembers { get; set; }

    public required bool Enabled { get; init; }

    public required int MembershipDisabledAfterDaysExpired { get; init; }

    public required int MembershipExpiringWarningDays { get; init; }

    public required int TrialPeriodMonths { get; init; }
}
