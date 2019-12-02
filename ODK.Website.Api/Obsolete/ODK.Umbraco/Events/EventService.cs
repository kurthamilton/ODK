using System;
using System.Collections.Generic;
using System.Linq;
using ODK.Core.Events;
using ODK.Umbraco.Members;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Umbraco.Events
{
    public class EventService
    {
        private readonly IContentService _contentService;
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository, IContentService contentService)
        {
            _contentService = contentService;
            _eventRepository = eventRepository;
        }
        
        public ServiceResult CreateEvent(IPublishedContent chapter, int userId, string name, string location, DateTime date, string time, string imageUrl, string address,
            string mapQuery, string description)
        {
            IPublishedContent eventsPage = chapter.GetPropertyValue<IPublishedContent>("eventsPage");

            IContent @event = _contentService.CreateContent(name, eventsPage.Id, "event", userId);

            @event.SetValue(EventPropertyNames.Location, location);
            @event.SetValue(EventPropertyNames.Date, date);

            if (!string.IsNullOrEmpty(time))
            {
                @event.SetValue(EventPropertyNames.Time, time);
            }

            if (!string.IsNullOrEmpty(imageUrl))
            {
                @event.SetValue(EventPropertyNames.ImageUrl, imageUrl);
            }

            if (!string.IsNullOrEmpty(address))
            {
                @event.SetValue(EventPropertyNames.Address, address);
            }

            if (!string.IsNullOrEmpty(mapQuery))
            {
                @event.SetValue(EventPropertyNames.MapQuery, mapQuery);
            }

            if (!string.IsNullOrEmpty(description))
            {
                @event.SetValue(EventPropertyNames.Description, description);
            }

            @event.SetValue(EventPropertyNames.Public, false);

            _contentService.Publish(@event, userId);

            return new ServiceResult(true);
        }

        public IEvent GetEvent(IPublishedContent @event)
        {
            return new EventModel(@event, ReplaceEventProperties);
        }

        public Dictionary<EventResponseType, IReadOnlyCollection<OdkMember>> GetEventResponses(int eventId, UmbracoHelper helper)
        {
            IReadOnlyCollection<EventResponse> responses = _eventRepository.GetEventResponses(eventId);

            Dictionary<EventResponseType, List<OdkMember>> dictionary = new Dictionary<EventResponseType, List<OdkMember>>();
            foreach (EventResponse response in responses)
            {
                EventResponseType responseType = (EventResponseType)response.ResponseTypeId;
                if (!dictionary.ContainsKey(responseType))
                {
                    dictionary.Add(responseType, new List<OdkMember>());
                }

                IPublishedContent umbracoMember = helper.TypedMember(response.MemberId);
                if (umbracoMember != null)
                {
                    OdkMember member = new MemberModel(umbracoMember);
                    dictionary[responseType].Add(member);
                }
            }

            return dictionary.ToDictionary(x => x.Key, x => (IReadOnlyCollection<OdkMember>)x.Value.ToArray());
        }

        public Dictionary<int, EventResponseType> GetMemberResponses(int memberId, IEnumerable<int> eventIds)
        {
            IReadOnlyCollection<EventResponse> responses = _eventRepository.GetMemberResponses(memberId, eventIds);
            return responses.ToDictionary(x => x.EventId, x => (EventResponseType)x.ResponseTypeId);
        }

        public IEnumerable<IEvent> GetNextEvents(IPublishedContent eventsPage, OdkMember currentMember, int maxItems = 0)
        {
            EventSearchCriteria criteria = new EventSearchCriteria
            {
                FutureOnly = true,
                MaxItems = maxItems
            };

            IEnumerable<IEvent> nextEvents = SearchEvents(eventsPage, currentMember, criteria);
            return nextEvents;
        }

        public ServiceResult HasTicketsAvailable(IEvent @event)
        {
            if (!IsTicketedEvent(@event))
            {
                return new ServiceResult(false, "Event is not ticketed");
            }

            if (@event.TicketCount == null)
            {
                return new ServiceResult(false, "No tickets are available");
            }

            if (@event.TicketDeadline != null && DateTime.Today > @event.TicketDeadline)
            {
                return new ServiceResult(false, "The ticket deadline has passed");
            }

            IReadOnlyCollection<EventResponse> responses = _eventRepository.GetEventResponses(@event.Id)
                .Where(x => x.ResponseTypeId == (int)EventResponseType.Yes)
                .ToArray();

            int ticketsAvailable = @event.TicketCount.Value - responses.Count;
            if (ticketsAvailable <= 0)
            {
                return new ServiceResult(false, "No tickets are available");
            }

            return new ServiceResult(true);
        }

        public bool IsTicketedEvent(IEvent @event)
        {
            return @event?.TicketCost != null;
        }

        public void LogSentEventInvite(int eventId, UmbracoHelper helper)
        {
            IContent @event = _contentService.GetById(eventId);
            @event.SetValue(EventPropertyNames.InviteSentDate, DateTime.Now);
            _contentService.SaveAndPublishWithStatus(@event);
        }

        public IEnumerable<IEvent> SearchEvents(IPublishedContent eventsPage, OdkMember member, EventSearchCriteria criteria)
        {
            IEnumerable<IEvent> events = eventsPage
                .Children
                .Select(GetEvent)
                .Where(x => member != null || x.IsPublic)
                .OrderBy(x => x.Date);

            if (criteria.FutureOnly == true)
            {
                events = events.Where(x => x.Date >= DateTime.Today);
            }

            if (criteria.Month != null)
            {
                events = events.Where(x => x.Date.Month == criteria.Month.Value);
            }

            if (criteria.MaxItems > 0)
            {
                events = events.Take(criteria.MaxItems.Value);
            }

            return events;
        }

        public void UpdateEventResponse(IPublishedContent @event, OdkMember member, EventResponseType responseType)
        {
            if (@event == null)
            {
                return;
            }            

            if (!Enum.IsDefined(typeof(EventResponseType), responseType) || responseType == EventResponseType.None)
            {
                return;
            }

            IEvent eventModel = GetEvent(@event);            

            if (eventModel.Date == DateTime.MinValue)
            {
                return;
            }

            _eventRepository.UpdateEventResponse(new EventResponse
            {
                EventId = @event.Id,
                MemberId = member.Id,
                ResponseTypeId = (int)responseType
            });
        }

        private static string ReplaceEventProperties(string text, IEvent eventModel)
        {
            return text
                .Replace("{{Name}}", eventModel.Name)
                .Replace("{{eventUrl}}", eventModel.Url)
                .Replace($"{{{{{EventPropertyNames.Address}}}}}", eventModel.Address)
                .Replace($"{{{{{EventPropertyNames.Date}}}}}", eventModel.Date.ToString("dddd dd MMMM, yyyy"))
                .Replace($"{{{{{EventPropertyNames.Description}}}}}", eventModel.Description)
                .Replace($"{{{{{EventPropertyNames.Location}}}}}", eventModel.Location)
                .Replace($"{{{{{EventPropertyNames.Time}}}}}", eventModel.Time);
        }
    }
}
