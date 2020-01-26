using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Venues;

namespace ODK.Services.Venues
{
    public interface IVenueAdminService
    {
        Task<Venue> CreateVenue(Guid currentMemberId, CreateVenue venue);

        Task<IReadOnlyCollection<VenueStats>> GetChapterVenueStats(Guid currentMemberId, Guid chapterId);

        Task<Venue> GetVenue(Guid currentMemberId, Guid venueId);

        Task<IReadOnlyCollection<Venue>> GetVenues(Guid currentMemberId, Guid chapterId);

        Task<Venue> UpdateVenue(Guid memberId, Guid id, CreateVenue venue);
    }
}
