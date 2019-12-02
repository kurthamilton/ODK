using System;

namespace ODK.Web.Api.Account.Requests
{
    public class CreateMemberProfileApiRequest : UpdateMemberProfileApiRequest
    {
        public Guid ChapterId { get; set; }

        public string EmailAddress { get; set; }
    }
}
