using System;
using System.Collections.Generic;
using System.Linq;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public class MemberProfile
    {
        public MemberProfile(Member member, IEnumerable<MemberProperty> memberProperties)
            : this(member.EmailAddress, member.EmailOptIn, member.FirstName, member.LastName, member.CreatedDate.Date, memberProperties)
        {
        }

        public MemberProfile(string emailAddress, bool emailOptIn, string firstName, string lastName, DateTime joined, IEnumerable<MemberProperty> memberProperties)
        {
            EmailAddress = emailAddress.Trim();
            Joined = joined;
            MemberProperties = memberProperties.ToArray();

            Update(emailOptIn, firstName, lastName);
        }

        public string EmailAddress { get; }

        public bool EmailOptIn { get; private set; }

        public string FirstName { get; private set; }

        public DateTime Joined { get; }

        public string LastName { get; private set; }

        public IReadOnlyCollection<MemberProperty> MemberProperties { get; }

        public void Update(bool emailOptIn, string firstName, string lastName)
        {
            EmailOptIn = emailOptIn;
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }
    }
}
