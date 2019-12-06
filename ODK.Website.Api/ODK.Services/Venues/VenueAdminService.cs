using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Venues;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Venues
{
    public class VenueAdminService : OdkAdminServiceBase, IVenueAdminService
    {
        private readonly ICacheService _cacheService;
        private readonly IVenueRepository _venueRepository;

        public VenueAdminService(IChapterRepository chapterRepository, IVenueRepository venueRepository, ICacheService cacheService)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _venueRepository = venueRepository;
        }

        public async Task<Venue> CreateVenue(Guid currentMemberId, CreateVenue venue)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);

            Venue create = new Venue(Guid.Empty, venue.ChapterId, venue.Name, venue.Address, venue.MapQuery);

            await ValidateVenue(create);

            Guid id = await _venueRepository.CreateVenue(create);

            _cacheService.RemoveVersionedCollection<Venue>();

            return await _venueRepository.GetVenue(id);
        }

        private async Task ValidateVenue(Venue venue)
        {
            if (string.IsNullOrWhiteSpace(venue.Name))
            {
                throw new OdkServiceException("Name required");
            }

            Venue existing = await _venueRepository.GetVenueByName(venue.Name);
            if (existing != null && existing.Id != venue.Id)
            {
                throw new OdkServiceException("Venue with that name already exists");
            }
        }
    }
}
