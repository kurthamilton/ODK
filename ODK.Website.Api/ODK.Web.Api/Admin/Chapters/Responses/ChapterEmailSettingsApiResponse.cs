namespace ODK.Web.Api.Admin.Chapters.Responses
{
    public class ChapterEmailSettingsApiResponse
    {
        public string AdminEmailAddress { get; set; }

        public string ContactEmailAddress { get; set; }

        public string EmailApiKey { get; set; }

        public string EmailProvider { get; set; }

        public string FromEmailAddress { get; set; }

        public string FromEmailName { get; set; }
    }
}
