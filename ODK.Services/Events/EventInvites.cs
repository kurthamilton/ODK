using System;

namespace ODK.Services.Events
{
    public class EventInvites
    {
        public int Delivered { get; set; }

        public Guid EventId { get; set; }

        public int Sent { get; set; }
    }
}
