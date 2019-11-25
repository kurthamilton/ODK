using ODK.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Web;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Umbraco.Payments
{
    public class EventPaymentModel : PaymentModel
    {
        public EventPaymentModel(IPublishedContent content, IPublishedContent homePage, OdkMember member, IEvent @event)
            : base(content, homePage, member)
        {
            Amount = content.GetPropertyValue<double>("eventTicketCost");
            Description = @event.Name;
            Title = @event.Name;
        }

        public override double Amount { get; }

        public override string Description { get; }
        
        public override string Title { get; }
    }
}
