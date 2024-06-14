using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Sql;

namespace ODK.Data.Repositories;

public class VenueRepository : RepositoryBase, IVenueRepository
{
    public VenueRepository(SqlContext context)
        : base(context)
    {
    }

    public async Task<Guid> CreateVenue(Venue venue)
    {
        return await Context
            .Insert(venue)
            .GetIdentityAsync();
    }

    public async Task<Venue> GetPublicVenue(Guid id)
    {
        return await Context
            .Select<Venue>()
            .Join<Event, Guid>(x => x.Id, x => x.VenueId)
            .Where(x => x.Id).EqualTo(id)
            .Where<Event, bool>(x => x.IsPublic).EqualTo(true)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Venue> GetVenue(Guid id)
    {
        return await Context
            .Select<Venue>()
            .Where(x => x.Id).EqualTo(id)
            .FirstOrDefaultAsync();
    }

    public async Task<Venue> GetVenueByName(string name)
    {
        return await Context
            .Select<Venue>()
            .Where(x => x.Name).EqualTo(name)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<Venue>> GetVenues(Guid chapterId)
    {
        return await Context
            .Select<Venue>()
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<Venue>> GetVenues(Guid chapterId, IEnumerable<Guid> venueIds)
    {
        return await Context
            .Select<Venue>()
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .Where(x => x.Id).In(venueIds)
            .ToArrayAsync();
    }
    
    public async Task UpdateVenue(Venue venue)
    {
        await Context
            .Update<Venue>()
            .Set(x => x.Name, venue.Name)
            .Set(x => x.Address, venue.Address)
            .Set(x => x.MapQuery, venue.MapQuery)
            .Where(x => x.Id).EqualTo(venue.Id)
            .ExecuteAsync();
    }
}
