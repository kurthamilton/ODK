using System;
using Microsoft.AspNetCore.Http;

namespace ODK.Web.Common.Account.Requests
{
    public class CreateMemberProfileApiRequest : UpdateMemberProfileApiRequest
    {
        public Guid ChapterId { get; set; }

        public string EmailAddress { get; set; }

        public IFormFile Image { get; set; }
    }
}
