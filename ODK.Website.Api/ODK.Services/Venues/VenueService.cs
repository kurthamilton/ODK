using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

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

        public async Task<VersionedServiceResult<IReadOnlyCollection<Venue>>> GetPublicVenues(long? currentVersion, Guid chapterId)
        {
            return await _cacheService.GetOrSetVersionedCollection(
                () => _venueRepository.GetPublicVenues(chapterId),
                () => _venueRepository.GetPublicVenuesVersion(chapterId),
                currentVersion,
                $"public-{chapterId}");
        }

        public async Task<VersionedServiceResult<Venue>> GetVenue(long? currentVersion, Guid? currentMemberId, Guid id)
        {
            Venue venue;

            if (currentMemberId != null)
            {
                venue = await _venueRepository.GetVenue(id);
            }
            else
            {
                venue = await _venueRepository.GetPublicVenue(id);
            }

            if (venue == null)
            {
                throw new OdkNotFoundException();
            }

            if (currentMemberId != null)
            {
                await _authorizationService.AssertMemberIsChapterMember(currentMemberId.Value, venue.ChapterId);
            }

            return await _cacheService.GetOrSetVersionedItem(
                () => _venueRepository.GetVenue(id),
                id,
                currentVersion);
        }

        public async Task<Venue> GetVenue(Member currentMember, Guid venueId)
        {
            Venue venue = await _venueRepository.GetVenue(venueId);
            if (venue == null || venue.ChapterId != currentMember?.ChapterId)
            {
                return null;
            }

            return venue;
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<Venue>>> GetVenues(long? currentVersion, Guid currentMemberId, Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, chapterId);

            return await _cacheService.GetOrSetVersionedCollection(
                () => _venueRepository.GetVenues(chapterId),
                () => _venueRepository.GetVenuesVersion(chapterId),
                currentVersion,
                chapterId);
        }
    }
}
