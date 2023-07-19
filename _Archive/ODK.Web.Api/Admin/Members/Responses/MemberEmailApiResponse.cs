using System;

namespace ODK.Web.Api.Admin.Members.Responses
{
    public class MemberEmailApiResponse
    {
        public string EmailAddress { get; set; }

        public Guid MemberId { get; set; }
    }
}
