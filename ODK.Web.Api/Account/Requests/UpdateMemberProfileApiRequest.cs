using System.Collections.Generic;

namespace ODK.Web.Api.Account.Requests
{
    public class UpdateMemberProfileApiRequest
    {
        public string EmailAddress { get; set; }

        public bool EmailOptIn { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public IEnumerable<UpdateMemberPropertyApiRequest> Properties { get; set; }
    }
}
