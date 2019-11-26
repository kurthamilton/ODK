using System;

namespace ODK.Core.Members
{
    public class MemberGroup
    {
        public MemberGroup(Guid id, Guid chapterId, string name)
        {
            ChapterId = chapterId;
            Id = id;
            Name = name;
        }

        public Guid ChapterId { get; }

        public Guid Id { get; }

        public string Name { get; private set; }

        public void Update(string name)
        {
            Name = name;
        }
    }
}
