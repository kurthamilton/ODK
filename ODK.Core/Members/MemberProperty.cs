using System;

namespace ODK.Core.Members
{
    public class MemberProperty
    {
        public MemberProperty(Guid id, Guid memberId, Guid chapterPropertyId, string value)
        {
            ChapterPropertyId = chapterPropertyId;
            Id = id;
            MemberId = memberId;
            Value = value;
        }

        public Guid ChapterPropertyId { get; }

        public Guid Id { get; }

        public Guid MemberId { get; }

        public string Value { get; private set; }

        public void Update(string value)
        {
            Value = value;
        }
    }
}
