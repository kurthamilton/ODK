using System.Linq;
using System.Web.Mvc;
using ODK.Core.Events;
using ODK.Umbraco.Events;
using ODK.Umbraco.Payments;
using ODK.Umbraco.Web.Mvc;
using ODK.Website.Models;
using Umbraco.Core.Models;

namespace ODK.Website.Controllers
{
    public class EventsController : OdkSurfaceControllerBase
    {
        private readonly EventService _eventService;

        public EventsController(EventService eventService)
        {
            _eventService = eventService;
        }

        [ChildActionOnly]
        [HttpGet]
        public ActionResult EventSidebar(int eventId)
        {
            return EventSidebarView(eventId);
        }

        [HttpPost]
        public ActionResult EventSidebar(int eventId, EventResponseType responseType)
        {
            if (CurrentMember == null)
            {
                return RedirectToCurrentUmbracoPage();
            }

            IPublishedContent content = Umbraco.TypedContent(eventId);
            IEvent @event = _eventService.GetEvent(content);

            if (_eventService.IsTicketedEvent(@event))
            {
                return RedirectToCurrentUmbracoPage();
            }

            _eventService.UpdateEventResponse(Umbraco.TypedContent(eventId), CurrentMember, responseType);

            return EventSidebarView(eventId);
        }

        private ActionResult EventSidebarView(int eventId)
        {
            EventSidebarViewModel viewModel = new EventSidebarViewModel
            {
                EventId = eventId,
                Helper = Umbraco
            };

            IPublishedContent content = Umbraco.TypedContent(eventId);
            IEvent @event = _eventService.GetEvent(content);

            if (CurrentUmbracoMember != null)
            {
                viewModel.MemberId = CurrentUmbracoMember.Id;
                viewModel.MemberResponses = _eventService.GetEventResponses(eventId, Umbraco);

                if (@event.TicketCost != null)
                {
                    viewModel.EventPaymentModel = new EventPaymentModel(content, HomePage, CurrentMember, @event);

                    viewModel.TicketCost = @event.TicketCost;
                    viewModel.TicketCount = @event.TicketCount;
                    viewModel.TicketDeadline = @event.TicketDeadline;

                    if (viewModel.TicketCount != null)
                    {
                        viewModel.TicketsRemaining = viewModel.TicketCount - 
                            (viewModel.MemberResponses.ContainsKey(EventResponseType.Yes) ? viewModel.MemberResponses[EventResponseType.Yes].Count : 0);
                    }
                }                

                foreach (EventResponseType key in viewModel.MemberResponses.Keys)
                {
                    if (viewModel.MemberResponses[key].Any(x => x.Id == CurrentUmbracoMember.Id))
                    {
                        viewModel.MemberResponse = key;
                        break;
                    }
                }
            }

            return PartialView("Events/EventSidebar", viewModel);
        }
    }
}