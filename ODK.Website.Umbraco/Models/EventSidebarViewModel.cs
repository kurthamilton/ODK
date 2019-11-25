using System;
using System.Collections.Generic;
using ODK.Core.Events;
using ODK.Core.Payments;
using Umbraco.Web;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Website.Models
{
    public class EventSidebarViewModel
    {
        public int EventId { get; set; }

        public UmbracoHelper Helper { get; set; }

        public int MemberId { get; set; }

        public EventResponseType MemberResponse { get; set; }

        public Dictionary<EventResponseType, IReadOnlyCollection<OdkMember>> MemberResponses { get; set; }
            = new Dictionary<EventResponseType, IReadOnlyCollection<OdkMember>>();

        public IPayment EventPaymentModel { get; set; }

        public double? TicketCost { get; set; }

        public int? TicketCount { get; set; }

        public DateTime? TicketDeadline { get; set; }

        public int? TicketsRemaining { get; set; }
    }
}