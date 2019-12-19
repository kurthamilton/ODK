using System;

namespace ODK.Core.Mail
{
    public class MemberEmail
    {
        public MemberEmail(Guid id, Guid chapterId, string toAddress, string subject, string body, DateTime createdDate)
        {
            Body = body;
            ChapterId = chapterId;
            CreatedDate = createdDate;
            Id = id;
            Subject = subject;
            ToAddress = toAddress;
        }

        public string Body { get; }

        public Guid ChapterId { get; }

        public DateTime CreatedDate { get; }

        public Guid Id { get; private set; }

        public string Subject { get; }

        public string ToAddress { get; }

        public void SetId(Guid id)
        {
            if (Id != Guid.Empty)
            {
                throw new InvalidOperationException("Id has already been set");
            }

            Id = id;
        }
    }
}
