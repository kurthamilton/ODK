using System;

namespace ODK.Web.Api.Admin.Chapters.Responses
{
    public class ChapterAdminMemberApiResponse
    {
        public string AdminEmailAddress { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Guid MemberId { get; set; }

        public bool ReceiveContactEmails { get; set; }

        public bool ReceiveNewMemberEmails { get; set; }

        public bool SendNewMemberEmails { get; set; }
    }
}
