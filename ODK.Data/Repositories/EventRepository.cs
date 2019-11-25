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

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, DateTime after)
        {
            return await Context
                .Select<Event>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.Date).GreaterThanOrEqualTo(after)
                .ToArrayAsync();
        }

        public IReadOnlyCollection<EventResponse> GetEventResponses(int eventId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<EventResponse> GetMemberResponses(int memberId, IEnumerable<int> eventIds)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId, DateTime after)
        {
            return await Context
                .Select<Event>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.Date).GreaterThanOrEqualTo(after)
                .Where(x => x.IsPublic).EqualTo(true)
                .ToArrayAsync();
        }

        public void UpdateEventResponse(EventResponse eventResponse)
        {
            throw new NotImplementedException();
        }
    }
}
