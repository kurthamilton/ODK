using System;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Services.Venues
{
    public interface IVenueService
    {
        Task<Venue?> GetVenue(Member? currentMember, Guid venueId);
    }
}
