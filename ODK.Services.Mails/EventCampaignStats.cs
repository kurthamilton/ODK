using System;

namespace ODK.Services.Emails
{
    public class EventCampaignStats
    {
        public Guid EventId { get; set; }

        public int Sent { get; set; }
    }
}
