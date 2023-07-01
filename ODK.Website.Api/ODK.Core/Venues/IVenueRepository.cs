using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Venues
{
    public interface IVenueRepository
    {
        Task<Guid> CreateVenue(Venue venue);

        Task<Venue> GetPublicVenue(Guid id);

        Task<IReadOnlyCollection<Venue>> GetPublicVenues(Guid chapterId);

        Task<long> GetPublicVenuesVersion(Guid chapterId);

        Task<Venue> GetVenue(Guid id);

        Task<Venue> GetVenueByName(string name);

        Task<IReadOnlyCollection<Venue>> GetVenues(Guid chapterId);
        
        Task<long> GetVenuesVersion(Guid chapterId);

        Task UpdateVenue(Venue venue);
    }
}
