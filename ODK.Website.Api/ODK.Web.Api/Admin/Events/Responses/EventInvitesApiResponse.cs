using System;

namespace ODK.Web.Api.Admin.Events.Responses
{
    public class EventInvitesApiResponse
    {
        public Guid EventId { get; set; }

        public int Sent { get; set; }

        public DateTime? SentDate { get; set; }
    }
}
