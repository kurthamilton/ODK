using System.Collections.Generic;
using ODK.Core.Events;

namespace ODK.Web.Api.Admin.Events.Requests
{
    public class SendEventInviteeEmailApiRequest
    {
        public string Body { get; set; }

        public string Subject { get; set; }

        public IEnumerable<EventResponseType> Statuses { get; set; }
    }
}
