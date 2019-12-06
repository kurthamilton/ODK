using System;
using System.Threading.Tasks;
using ODK.Core.Venues;

namespace ODK.Services.Venues
{
    public interface IVenueAdminService
    {
        Task<Venue> CreateVenue(Guid currentMemberId, CreateVenue venue);

        Task<Venue> GetVenue(Guid currentMemberId, Guid venueId);

        Task<Venue> UpdateVenue(Guid memberId, Guid id, CreateVenue venue);
    }
}
