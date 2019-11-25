using System;

namespace ODK.Web.Api.Members.Responses
{
    public class MemberApiResponse
    {
        public Guid ChapterId { get; set; }

        public string FirstName { get; set; }

        public Guid Id { get; set; }

        public string ImageUrl { get; set; }

        public string LastName { get; set; }
    }
}
