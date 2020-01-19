using System;

namespace ODK.Web.Common.Events.Responses
{
    public class EventResponseApiResponse
    {
        public Guid EventId { get; set; }

        public Guid MemberId { get; set; }

        public int ResponseTypeId { get; set; }
    }
}
