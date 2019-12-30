using System;

namespace ODK.Web.Api.Events.Responses
{
    public class EventMemberResponseApiResponse
    {
        public Guid EventId { get; set; }

        public Guid MemberId { get; set; }

        public int ResponseTypeId { get; set; }
    }
}
