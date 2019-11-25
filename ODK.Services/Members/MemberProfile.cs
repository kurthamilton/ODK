using System;
using System.Collections.Generic;
using System.Linq;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public class MemberProfile
    {
        public MemberProfile(Member member, IEnumerable<MemberProperty> memberProperties)
        {
            EmailAddress = member.EmailAddress;
            EmailOptIn = member.EmailOptIn;
            FirstName = member.FirstName;
            Joined = member.CreatedDate.Date;
            LastName = member.LastName;
            MemberProperties = memberProperties.ToArray();
        }        

        public string EmailAddress { get; private set; }

        public bool EmailOptIn { get; private set; }

        public string FirstName { get; private set; }

        public DateTime Joined { get; }

        public string LastName { get; private set; }

        public IReadOnlyCollection<MemberProperty> MemberProperties { get; }

        public void Update(string emailAddress, bool emailOptIn, string firstName, string lastName)
        {
            EmailAddress = emailAddress;
            EmailOptIn = emailOptIn;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
