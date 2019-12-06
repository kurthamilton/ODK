using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Venues;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
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

        public async Task<long> GetVenuesVersion(Guid chapterId)
        {
            return await Context
                .Select<Venue>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .VersionAsync();
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
}
