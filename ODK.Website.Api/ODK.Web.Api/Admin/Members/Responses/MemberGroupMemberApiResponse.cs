using System;

namespace ODK.Web.Api.Admin.Members.Responses
{
    public class MemberGroupMemberApiResponse
    {
        public Guid MemberGroupId { get; set; }

        public Guid MemberId { get; set; }
    }
}
