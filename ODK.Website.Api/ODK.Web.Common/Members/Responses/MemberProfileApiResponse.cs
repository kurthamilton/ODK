using System;
using System.Collections.Generic;

namespace ODK.Web.Common.Members.Responses
{
    public class MemberProfileApiResponse
    {
        public string FirstName { get; set; }

        public DateTime Joined { get; set; }

        public string LastName { get; set; }

        public IEnumerable<MemberProfilePropertyApiResponse> Properties { get; set; }
    }
}
