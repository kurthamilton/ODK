using System;

namespace ODK.Web.Common.Members.Responses
{
    public class MemberApiResponse
    {
        public Guid ChapterId { get; set; }

        public string FirstName { get; set; }

        public Guid Id { get; set; }

        public string LastName { get; set; }
    }
}
