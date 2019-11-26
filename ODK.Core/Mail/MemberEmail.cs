using System;

namespace ODK.Core.Mail
{
    public class MemberEmail
    {
        public MemberEmail(Guid id, Guid chapterId, string toAddress, string subject, DateTime createdDate, bool sent)
        {
            ChapterId = chapterId;
            CreatedDate = createdDate;
            Id = id;
            Sent = sent;
            Subject = subject;
            ToAddress = toAddress;
        }

        public Guid ChapterId { get; }

        public DateTime CreatedDate { get; }

        public Guid Id { get; }

        public bool Sent { get; }

        public string Subject { get; }

        public string ToAddress { get; }
    }
}
