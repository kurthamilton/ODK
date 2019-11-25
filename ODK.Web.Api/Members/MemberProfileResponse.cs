using System;
using System.Collections.Generic;

namespace ODK.Web.Api.Members
{
    public class MemberProfileResponse
    {
        public string FirstName { get; set; }

        public DateTime Joined { get; set; }

        public string LastName { get; set; }

        public IEnumerable<MemberProfilePropertyResponse> Properties { get; set; }
    }
}
