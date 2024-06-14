using System;
using System.Collections.Generic;
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

        public VenueAdminService(IChapterRepository chapterRepository, IVenueRepository venueRepository,
            ICacheService cacheService)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _venueRepository = venueRepository;
        }

        public async Task<ServiceResult> CreateVenue(Guid currentMemberId, CreateVenue venue)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);

            Venue create = new Venue(Guid.Empty, venue.ChapterId, venue.Name, venue.Address, venue.MapQuery, 0);

            ServiceResult validationResult = await ValidateVenue(create);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            await _venueRepository.CreateVenue(create);

            _cacheService.RemoveVersionedCollection<Venue>(venue.ChapterId);

            return ServiceResult.Successful();
        }
        
        public async Task<Venue> GetVenue(Guid currentMemberId, Guid venueId)
        {
            Venue? venue = await _venueRepository.GetVenue(venueId);
            if (venue == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);

            return venue;
        }

        public async Task<IReadOnlyCollection<Venue>> GetVenues(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _venueRepository.GetVenues(chapterId);
        }

        public async Task<IReadOnlyCollection<Venue>> GetVenues(Guid currentMemberId, Guid chapterId,
            IReadOnlyCollection<Guid> venueIds)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _venueRepository.GetVenues(chapterId, venueIds);
        }

        public async Task<ServiceResult> UpdateVenue(Guid memberId, Guid id, CreateVenue venue)
        {
            Venue update = await GetVenue(memberId, id);

            update.Update(venue.Name, venue.Address, venue.MapQuery);

            ServiceResult validationResult = await ValidateVenue(update);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            await _venueRepository.UpdateVenue(update);

            _cacheService.RemoveVersionedItem<Venue>(id);
            _cacheService.RemoveVersionedCollection<Venue>(update.ChapterId);

            return ServiceResult.Successful();
        }
        
        private async Task<ServiceResult> ValidateVenue(Venue venue)
        {
            if (string.IsNullOrWhiteSpace(venue.Name))
            {
                return ServiceResult.Failure("Name required");
            }

            Venue? existing = await _venueRepository.GetVenueByName(venue.Name);
            if (existing != null && existing.Id != venue.Id)
            {
                return ServiceResult.Failure("Venue with that name already exists");
            }

            return ServiceResult.Successful();
        }
    }
}
