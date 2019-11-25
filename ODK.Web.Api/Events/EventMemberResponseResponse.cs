using System;

namespace ODK.Web.Api.Events
{
    public class EventMemberResponseResponse
    {
        public Guid EventId { get; set; }

        public Guid MemberId { get; set; }

        public int ResponseTypeId { get; set; }
    }
}
