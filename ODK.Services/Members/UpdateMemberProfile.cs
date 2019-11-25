using System;
using System.Collections.Generic;

namespace ODK.Services.Members
{
    public class UpdateMemberProfile
    {
        public string EmailAddress { get; set; }

        public bool EmailOptIn { get; set; }

        public string FirstName { get; set; }

        public Guid Id { get; set; }

        public string LastName { get; set; }

        public IEnumerable<UpdateMemberProperty> Properties { get; set; }
    }
}
