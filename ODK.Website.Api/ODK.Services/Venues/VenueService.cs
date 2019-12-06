using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Venues;
using ODK.Services.Authorization;
using ODK.Services.Caching;

namespace ODK.Services.Venues
{
    public class VenueService : IVenueService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheService _cacheService;
        private readonly IVenueRepository _venueRepository;

        public VenueService(IVenueRepository venueRepository, ICacheService cacheService, IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            _cacheService = cacheService;
            _venueRepository = venueRepository;
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<Venue>>> GetVenues(long? currentVersion, Guid currentMemberId, Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, chapterId);

            return await _cacheService.GetOrSetVersionedCollection(
                () => _venueRepository.GetVenues(chapterId),
                () => _venueRepository.GetVenuesVersion(chapterId),
                currentVersion);
        }
    }
}
