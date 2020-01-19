namespace ODK.Web.Api.Admin.Chapters.Requests
{
    public class UpdateChapterPropertyApiRequest
    {
        public string HelpText { get; set; }

        public string Label { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public string Subtitle { get; set; }
    }
}
