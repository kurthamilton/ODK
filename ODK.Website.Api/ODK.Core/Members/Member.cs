using System;

namespace ODK.Core.Members
{
    public class Member : IVersioned
    {
        public Member(Guid id, Guid chapterId, string emailAddress, bool emailOptIn,
            string firstName, string lastName, DateTime createdDate, bool activated, bool disabled,
            long version)
        {
            Activated = activated;
            ChapterId = chapterId;
            CreatedDate = createdDate;
            Disabled = disabled;
            EmailAddress = emailAddress;
            EmailOptIn = emailOptIn;
            FirstName = firstName;
            Id = id;
            LastName = lastName;
            Version = version;
        }

        public bool Activated { get; }

        public Guid ChapterId { get; }

        public DateTime CreatedDate { get; }

        public bool Disabled { get; }

        public string EmailAddress { get; }

        public bool EmailOptIn { get; }

        public string FirstName { get; }

        public string FullName => $"{FirstName?.Trim()} {LastName?.Trim()}".Trim();

        public Guid Id { get; }

        public string LastName { get; }

        public long Version { get; }

        public bool CanBeViewedBy(Member currentMember)
        {
            return currentMember?.ChapterId == ChapterId;
        }
    }
}
