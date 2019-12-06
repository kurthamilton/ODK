using System;
using System.Threading.Tasks;
using ODK.Core.Venues;

namespace ODK.Services.Venues
{
    public interface IVenueAdminService
    {
        Task<Venue> CreateVenue(Guid currentMemberId, CreateVenue venue);
    }
}
