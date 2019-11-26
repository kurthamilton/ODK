using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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

        public async Task<Event> CreateEvent(Event @event)
        {
            Guid id = await Context.InsertAsync(@event);
            return await GetEvent(id);
        }

        public async Task<Event> GetEvent(Guid id)
        {
            return await Context
                .Select<Event>()
                .Where(x => x.Id).EqualTo(id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, DateTime? after)
        {
            return await Context
                .Select<Event>()
                .OrderBy(x => x.Date, SqlSortDirection.Descending)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.Date).GreaterThanOrEqualTo(after ?? SqlDateTime.MinValue.Value)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid eventId)
        {
            return await Context
                .Select<EventMemberResponse>()
                .Where(x => x.EventId).EqualTo(eventId)
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
                .Set(x => x.Address, @event.Address)
                .Set(x => x.Date, @event.Date)
                .Set(x => x.Description, @event.Description)
                .Set(x => x.ImageUrl, @event.ImageUrl)
                .Set(x => x.IsPublic, @event.IsPublic)
                .Set(x => x.Location, @event.Location)
                .Set(x => x.MapQuery, @event.MapQuery)
                .Set(x => x.Name, @event.Name)
                .Set(x => x.Time, @event.Time)
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
                await Context.InsertAsync(response);
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
