namespace ODK.Web.Api.Admin.Chapters.Requests
{
    public class UpdateChapterEmailSettingsApiRequest
    {
        public string AdminEmailAddress { get; set; }

        public string ContactEmailAddress { get; set; }

        public string FromEmailAddress { get; set; }
    }
}
