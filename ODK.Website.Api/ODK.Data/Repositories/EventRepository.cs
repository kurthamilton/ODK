using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class EventRepository : RepositoryBase, IEventRepository
    {
        public EventRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task AddEventEmail(EventEmail eventEmail)
        {
            await Context
                .Insert(eventEmail)
                .ExecuteAsync();
        }

        public async Task<Event> CreateEvent(Event @event)
        {
            Guid id = await Context
                .Insert(@event)
                .GetIdentityAsync();

            return await GetEvent(id);
        }

        public async Task DeleteEvent(Guid id)
        {
            await Context
                .Delete<Event>()
                .Where(x => x.Id).EqualTo(id)
                .ExecuteAsync();
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetChapterResponses(Guid chapterId)
        {
            return await Context
                .Select<EventMemberResponse>()
                .Join<Event, Guid>(x => x.EventId, x => x.Id)
                .Where<Event, Guid>(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<Event> GetEvent(Guid id)
        {
            return await Context
                .Select<Event>()
                .Where(x => x.Id).EqualTo(id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetEventCount(Guid chapterId)
        {
            return await Context
                .Select<Event>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .CountAsync();
        }

        public async Task<EventEmail> GetEventEmail(Guid eventId)
        {
            return await Context
                .Select<EventEmail>()
                .Where(x => x.EventId).EqualTo(eventId)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<EventEmail>> GetEventEmails(Guid chapterId, DateTime after)
        {
            return await Context
                .Select<EventEmail>()
                .Join<Event, Guid>(x => x.EventId, x => x.Id)
                .Where<Event, Guid>(x => x.ChapterId).EqualTo(chapterId)
                .Where<Event, DateTime>(x => x.Date).GreaterThanOrEqualTo(after)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid eventId)
        {
            return await Context
                .Select<EventMemberResponse>()
                .Where(x => x.EventId).EqualTo(eventId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, DateTime after)
        {
            return await Context
                .Select<Event>()
                .OrderBy(x => x.Date, SqlSortDirection.Descending)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.Date).GreaterThanOrEqualTo(after)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, int page, int pageSize)
        {
            return await Context
                .Select<Event>()
                .Page(page, pageSize)
                .OrderBy(x => x.Date, SqlSortDirection.Descending)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid venueId)
        {
            return await Context
                .Select<Event>()
                .Where(x => x.VenueId).EqualTo(venueId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetMemberResponses(Guid memberId)
        {
            return await Context
                .Select<EventMemberResponse>()
                .Join<Event, Guid>(x => x.EventId, x => x.Id)
                .Where(x => x.MemberId).EqualTo(memberId)
                .Where<Event, DateTime>(x => x.Date).GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId, DateTime after)
        {
            return await Context
                .Select<Event>()
                .OrderBy(x => x.Date, SqlSortDirection.Descending)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.Date).GreaterThanOrEqualTo(after)
                .Where(x => x.IsPublic).EqualTo(true)
                .ToArrayAsync();
        }

        public async Task UpdateEvent(Event @event)
        {
            await Context
                .Update<Event>()
                .Set(x => x.Date, @event.Date)
                .Set(x => x.Description, @event.Description)
                .Set(x => x.ImageUrl, @event.ImageUrl)
                .Set(x => x.IsPublic, @event.IsPublic)
                .Set(x => x.Name, @event.Name)
                .Set(x => x.Time, @event.Time)
                .Set(x => x.VenueId, @event.VenueId)
                .Where(x => x.Id).EqualTo(@event.Id)
                .ExecuteAsync();
        }

        public async Task UpdateEventResponse(EventMemberResponse response)
        {
            if (await MemberHasRespondedToEvent(response.EventId, response.MemberId))
            {
                await Context
                    .Update<EventMemberResponse>()
                    .Set(x => x.ResponseTypeId, response.ResponseTypeId)
                    .Where(x => x.EventId).EqualTo(response.EventId)
                    .Where(x => x.MemberId).EqualTo(response.MemberId)
                    .ExecuteAsync();
            }
            else
            {
                await Context.Insert(response)
                    .ExecuteAsync();
            }
        }

        private async Task<bool> MemberHasRespondedToEvent(Guid eventId, Guid memberId)
        {
            return await Context
                .Select<EventMemberResponse>()
                .Where(x => x.EventId).EqualTo(eventId)
                .Where(x => x.MemberId).EqualTo(memberId)
                .CountAsync() > 0;
        }
    }
}
