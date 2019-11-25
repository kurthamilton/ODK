using System;

namespace ODK.Core.Members
{
    public class Member
    {
        public Member(Guid id, Guid chapterId, string emailAddress, bool emailOptIn, 
            string firstName, string lastName, DateTime createdDate, bool disabled)
        {
            ChapterId = chapterId;
            CreatedDate = createdDate;
            Disabled = disabled;
            EmailAddress = emailAddress;
            EmailOptIn = emailOptIn;
            FirstName = firstName;
            Id = id;
            LastName = lastName;
        }

        public Guid ChapterId { get; }

        public DateTime CreatedDate { get; }

        public bool Disabled { get; }

        public string EmailAddress { get; }

        public bool EmailOptIn { get; }

        public string FirstName { get; }

        public Guid Id { get; }

        public string LastName { get; }     
    }
}
