using System;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Services.Venues
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _venueRepository;

        public VenueService(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }
        
        public async Task<Venue?> GetVenue(Member? currentMember, Guid venueId)
        {
            Venue? venue = await _venueRepository.GetVenue(venueId);
            if (venue == null)
            {
                return null;
            }

            if (currentMember == null || venue.ChapterId != currentMember.ChapterId)
            {
                venue = await _venueRepository.GetPublicVenue(venueId);
            }
            
            return venue;
        }
    }
}
