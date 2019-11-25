using System;
using System.Collections.Generic;

namespace ODK.Web.Api.Account.Responses
{
    public class AccountProfileApiResponse
    {
        public string EmailAddress { get; set; }

        public bool EmailOptIn { get; set; }

        public string FirstName { get; set; }

        public DateTime Joined { get; set; }

        public string LastName { get; set; }

        public IEnumerable<AccountProfilePropertyApiResponse> Properties { get; set; }
    }
}
