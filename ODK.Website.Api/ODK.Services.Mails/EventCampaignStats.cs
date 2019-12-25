using System;

namespace ODK.Services.Mails
{
    public class EventCampaignStats
    {
        public Guid EventId { get; set; }

        public int Sent { get; set; }
    }
}
