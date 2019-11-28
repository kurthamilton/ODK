using System;

namespace ODK.Core.Chapters
{
    public class ContactRequest
    {
        public ContactRequest(Guid id, Guid chapterId, DateTime createdDate, string fromAddress, string message)
        {
            ChapterId = chapterId;
            CreatedDate = createdDate;
            FromAddress = fromAddress;
            Id = id;
            Message = message;
        }

        public Guid ChapterId { get; }

        public DateTime CreatedDate { get; }

        public string FromAddress { get; }

        public Guid Id { get; }

        public string Message { get; }
    }
}
