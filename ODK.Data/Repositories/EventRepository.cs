using System.Data.SqlTypes;
using ODK.Core.Events;
using ODK.Data.Sql;

namespace ODK.Data.Repositories;

public class EventRepository : RepositoryBase, IEventRepository
{
    public EventRepository(SqlContext context)
        : base(context)
    {
    }

    public async Task<Guid> AddEventEmailAsync(EventEmail eventEmail)
    {
        return await Context
            .Insert(eventEmail)
            .GetIdentityAsync();
    }

    public async Task AddEventInvitesAsync(Guid eventId, IEnumerable<Guid> memberIds, DateTime sentDate)
    {
        foreach (Guid memberId in memberIds)
        {
            await Context
                .Insert(new EventInvite(eventId, memberId, sentDate))
                .ExecuteAsync();
        }
    }

    public async Task<Event> CreateEventAsync(Event @event)
    {
        Guid id = await Context
            .Insert(@event)
            .GetIdentityAsync();

        var created = await GetEventAsync(id);
        if (created == null)
        {
            throw new Exception("Event creation failed");
        }

        return created;
    }

    public async Task DeleteEventAsync(Guid id)
    {
        await Context
            .Delete<Event>()
            .Where(x => x.Id).EqualTo(id)
            .ExecuteAsync();
    }

    public async Task<IReadOnlyCollection<EventInvite>> GetChapterInvitesAsync(Guid chapterId,
        IEnumerable<Guid> eventIds)
    {
        return await Context
            .Select<EventInvite>()
            .Join<Event, Guid>(x => x.EventId, x => x.Id)
            .Where<Event, Guid>(x => x.ChapterId).EqualTo(chapterId)
            .Where<Event, Guid>(x => x.Id).In(eventIds)
            .ToArrayAsync();

    }

    public async Task<IReadOnlyCollection<EventResponse>> GetChapterResponsesAsync(Guid chapterId)
    {
        return await Context
            .Select<EventResponse>()
            .Join<Event, Guid>(x => x.EventId, x => x.Id)
            .Where<Event, Guid>(x => x.ChapterId).EqualTo(chapterId)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<EventResponse>> GetChapterResponsesAsync(Guid chapterId,
        IEnumerable<Guid> eventIds)
    {
        return await Context
            .Select<EventResponse>()
            .Join<Event, Guid>(x => x.EventId, x => x.Id)
            .Where<Event, Guid>(x => x.ChapterId).EqualTo(chapterId)
            .Where<Event, Guid>(x => x.Id).In(eventIds)
            .ToArrayAsync();
    }

    public async Task<Event?> GetEventAsync(Guid id)
    {
        return await Context
            .Select<Event>()
            .Where(x => x.Id).EqualTo(id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<EventEmail?> GetEventEmailAsync(Guid eventId)
    {
        return await Context
            .Select<EventEmail>()
            .Where(x => x.EventId).EqualTo(eventId)
            .FirstOrDefaultAsync();
    }
    
    public async Task<IReadOnlyCollection<EventEmail>> GetEventEmailsAsync(Guid chapterId, IEnumerable<Guid> eventIds)
    {
        return await Context
            .Select<EventEmail>()
            .Join<Event, Guid>(x => x.EventId, x => x.Id)
            .Where<Event, Guid>(x => x.ChapterId).EqualTo(chapterId)
            .Where<Event, Guid>(x => x.Id).In(eventIds)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<EventInvite>> GetEventInvitesAsync(Guid eventId)
    {
        return await Context
            .Select<EventInvite>()
            .Where(x => x.EventId).EqualTo(eventId)
            .ToArrayAsync();
    }
    
    public async Task<IReadOnlyCollection<EventInvite>> GetEventInvitesForMemberIdAsync(Guid memberId)
    {
        return await Context
            .Select<EventInvite>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<EventResponse>> GetEventResponsesAsync(Guid eventId)
    {
        return await Context
            .Select<EventResponse>()
            .Where(x => x.EventId).EqualTo(eventId)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsAsync(Guid chapterId, DateTime? after)
    {
        var query = Context
            .Select<Event>()
            .Where(x => x.ChapterId)
            .EqualTo(chapterId);

        if (after != null)
        {
            var afterValue = after.Value;
            query = query
                .ConditionalWhere(x => x.Date, after != null).GreaterThanOrEqualTo(afterValue);
        }

        return await query
            .OrderBy(x => x.Date, SqlSortDirection.Descending)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsAsync(Guid chapterId, int page, int pageSize)
    {
        return await Context
            .Select<Event>()
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .OrderBy(x => x.Date, SqlSortDirection.Descending)
            .Page(page, pageSize)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsByVenueAsync(Guid venueId)
    {
        return await Context
            .Select<Event>()
            .Where(x => x.VenueId).EqualTo(venueId)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<EventResponse>> GetMemberResponsesAsync(Guid memberId, bool allEvents = false)
    {
        return await Context
            .Select<EventResponse>()
            .Join<Event, Guid>(x => x.EventId, x => x.Id)
            .Where(x => x.MemberId).EqualTo(memberId)
            .Where(x => x.ResponseTypeId).NotEqualTo(EventResponseType.None)
            .Where<Event, DateTime>(x => x.Date).GreaterThanOrEqualTo(allEvents ? SqlDateTime.MinValue.Value : DateTime.UtcNow.Date)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<Event>> GetPublicEventsAsync(Guid chapterId, DateTime? after)
    {
        var query = Context
            .Select<Event>()
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .Where(x => x.IsPublic).EqualTo(true);

        if (after != null)
        {
            var afterValue = after.Value;
            query = query
                .ConditionalWhere(x => x.Date, after != null).GreaterThanOrEqualTo(afterValue);
        }

        return await query
            .OrderBy(x => x.Date, SqlSortDirection.Descending)
            .ToArrayAsync();
    }

    public async Task UpdateEventAsync(Event @event)
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
    
    public async Task UpdateEventResponseAsync(EventResponse response)
    {
        if (await MemberEventResponseExists(response.EventId, response.MemberId))
        {
            await Context
                .Update<EventResponse>()
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

    private async Task<bool> MemberEventResponseExists(Guid eventId, Guid memberId)
    {
        return await Context
                   .Select<EventResponse>()
                   .Where(x => x.EventId).EqualTo(eventId)
                   .Where(x => x.MemberId).EqualTo(memberId)
                   .CountAsync() > 0;
    }
}
