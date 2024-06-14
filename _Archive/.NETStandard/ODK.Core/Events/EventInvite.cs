using System;

namespace ODK.Core.Events
{
    public class EventInvite
    {
        public EventInvite(Guid eventId, Guid memberId, DateTime sentDate)
        {
            EventId = eventId;
            MemberId = memberId;
            SentDate = sentDate;
        }

        public Guid EventId { get; }

        public Guid MemberId { get; }

        public DateTime SentDate { get; }
    }
}
