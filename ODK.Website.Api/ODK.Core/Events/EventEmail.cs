using System;
using System.Collections.Generic;
using System.Text;

namespace ODK.Core.Events
{
    public class EventEmail
    {
        public EventEmail(Guid id, Guid eventId, string emailProvider, string emailProviderEmailId, DateTime sentDate)
        {
            EmailProvider = emailProvider;
            EmailProviderEmailId = emailProviderEmailId;
            EventId = eventId;
            Id = id;
            SentDate = sentDate;
        }

        public string EmailProvider { get; }

        public string EmailProviderEmailId { get; }

        public Guid EventId { get; }

        public Guid Id { get; }

        public DateTime SentDate { get; }
    }
}
