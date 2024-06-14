using System;

namespace ODK.Core.Events
{
    public class EventInvites
    {        
        public Guid EventId { get; set; }

        public int Sent { get; set; }

        public DateTime? SentDate { get; set; }
    }
}
