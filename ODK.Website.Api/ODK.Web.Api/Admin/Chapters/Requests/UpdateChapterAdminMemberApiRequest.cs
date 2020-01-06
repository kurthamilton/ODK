namespace ODK.Web.Api.Admin.Chapters.Requests
{
    public class UpdateChapterAdminMemberApiRequest
    {
        public string AdminEmailAddress { get; set; }

        public bool ReceiveContactEmails { get; set; }

        public bool ReceiveNewMemberEmails { get; set; }

        public bool SendNewMemberEmails { get; set; }
    }
}
