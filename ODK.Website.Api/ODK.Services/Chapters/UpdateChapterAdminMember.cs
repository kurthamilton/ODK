namespace ODK.Services.Chapters
{
    public class UpdateChapterAdminMember
    {
        public string AdminEmailAddress { get; set; }

        public bool ReceiveContactEmails { get; set; }

        public bool ReceiveNewMemberEmails { get; set; }

        public bool SendEventEmails { get; set; }

        public bool SendNewMemberEmails { get; set; }
    }
}
