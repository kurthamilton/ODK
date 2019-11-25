using System;
using System.Collections.Generic;

namespace ODK.Web.Api.Account
{
    public class AccountProfileResponse
    {
        public string EmailAddress { get; set; }

        public bool EmailOptIn { get; set; }

        public string FirstName { get; set; }

        public DateTime Joined { get; set; }

        public string LastName { get; set; }

        public IEnumerable<AccountProfilePropertyResponse> Properties { get; set; }
    }
}
