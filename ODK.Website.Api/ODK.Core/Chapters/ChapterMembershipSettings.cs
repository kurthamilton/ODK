using System;

namespace ODK.Core.Chapters
{
    public class ChapterMembershipSettings
    {
        public ChapterMembershipSettings(Guid chapterId, int trialPeriodMonths,
            int membershipDisabledAfterDaysExpired)
        {
            ChapterId = chapterId;
            MembershipDisabledAfterDaysExpired = membershipDisabledAfterDaysExpired;
            TrialPeriodMonths = trialPeriodMonths;
        }

        public Guid ChapterId { get; }

        public int MembershipDisabledAfterDaysExpired { get; set; }

        public int TrialPeriodMonths { get; set; }
    }
}
