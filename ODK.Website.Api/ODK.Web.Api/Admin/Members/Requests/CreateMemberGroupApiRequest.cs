using System;

namespace ODK.Web.Api.Admin.Members.Requests
{
    public class CreateMemberGroupApiRequest
    {
        public Guid ChapterId { get; set; }

        public string Name { get; set; }
    }
}
