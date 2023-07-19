namespace ODK.Web.Api.Admin.Chapters.Requests
{
    public class UpdateChapterMembershipSettingsApiRequest
    {
        public int MembershipDisabledAfterDaysExpired { get; set; }

        public int TrialPeriodMonths { get; set; }
    }
}
