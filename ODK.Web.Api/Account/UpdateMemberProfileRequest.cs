using System.Collections.Generic;

namespace ODK.Web.Api.Account
{
    public class UpdateMemberProfileRequest
    {
        public string EmailAddress { get; set; }

        public bool EmailOptIn { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public IEnumerable<UpdateMemberPropertyRequest> Properties { get; set; }
    }
}
