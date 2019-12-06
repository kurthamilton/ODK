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

            Venue create = new Venue(Guid.Empty, venue.ChapterId, venue.Name, venue.Address, venue.MapQuery, 0);

            await ValidateVenue(create);

            Guid id = await _venueRepository.CreateVenue(create);

            _cacheService.RemoveVersionedCollection<Venue>();

            return await _venueRepository.GetVenue(id);
        }

        public async Task<Venue> GetVenue(Guid currentMemberId, Guid venueId)
        {
            Venue venue = await _venueRepository.GetVenue(venueId);
            if (venue == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);

            return venue;
        }

        public async Task<Venue> UpdateVenue(Guid memberId, Guid id, CreateVenue venue)
        {
            Venue update = await GetVenue(memberId, id);

            update.Update(venue.Name, venue.Address, venue.MapQuery);

            await ValidateVenue(update);

            await _venueRepository.UpdateVenue(update);

            _cacheService.RemoveVersionedItem<Venue>(id);
            _cacheService.RemoveVersionedCollection<Venue>();

            return update;
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
