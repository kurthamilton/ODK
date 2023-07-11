using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Venues
{
    public class VenueAdminService : OdkAdminServiceBase, IVenueAdminService
    {
        private readonly ICacheService _cacheService;
        private readonly IEventRepository _eventRepository;
        private readonly IVenueRepository _venueRepository;

        public VenueAdminService(IChapterRepository chapterRepository, IVenueRepository venueRepository,
            ICacheService cacheService, IEventRepository eventRepository)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _eventRepository = eventRepository;
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

        public async Task<Venue> CreateVenueOld(Guid currentMemberId, CreateVenue venue)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);

            Venue create = new Venue(Guid.Empty, venue.ChapterId, venue.Name, venue.Address, venue.MapQuery, 0);

            await AssertVenueIsValid(create);

            Guid id = await _venueRepository.CreateVenue(create);

            _cacheService.RemoveVersionedCollection<Venue>(venue.ChapterId);

            return await _venueRepository.GetVenue(id);
        }

        public async Task<IReadOnlyCollection<VenueStats>> GetChapterVenueStats(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            IReadOnlyCollection<Venue> venues = await _venueRepository.GetVenues(chapterId);
            IReadOnlyCollection<Event> events = await _eventRepository.GetEvents(chapterId, 1, int.MaxValue);
            IReadOnlyCollection<EventResponse> memberResponses = await _eventRepository.GetChapterResponses(chapterId);

            IDictionary<Guid, IReadOnlyCollection<Event>> venueEvents = events
                .GroupBy(x => x.VenueId)
                .ToDictionary(x => x.Key, x => (IReadOnlyCollection<Event>)x.ToArray());

            return venues
                .Select(x => new VenueStats
                {
                    EventCount = venueEvents.ContainsKey(x.Id) ? venueEvents[x.Id].Count : 0,
                    LastEventDate = venueEvents.ContainsKey(x.Id) ? venueEvents[x.Id].Max(e => e.Date) : new DateTime?(),
                    VenueId = x.Id
                })
                .ToArray();
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

        public async Task<Venue> UpdateVenueOld(Guid memberId, Guid id, CreateVenue venue)
        {
            Venue update = await GetVenue(memberId, id);

            update.Update(venue.Name, venue.Address, venue.MapQuery);

            await AssertVenueIsValid(update);

            await _venueRepository.UpdateVenue(update);

            _cacheService.RemoveVersionedItem<Venue>(id);
            _cacheService.RemoveVersionedCollection<Venue>(update.ChapterId);

            return update;
        }

        private async Task AssertVenueIsValid(Venue venue)
        {
            ServiceResult result = await ValidateVenue(venue);
            if (!result.Success)
            {
                throw new OdkServiceException(result.Message);
            }
        }

        private async Task<ServiceResult> ValidateVenue(Venue venue)
        {
            if (string.IsNullOrWhiteSpace(venue.Name))
            {
                return ServiceResult.Failure("Name required");
            }

            Venue existing = await _venueRepository.GetVenueByName(venue.Name);
            if (existing != null && existing.Id != venue.Id)
            {
                return ServiceResult.Failure("Venue with that name already exists");
            }

            return ServiceResult.Successful();
        }
    }
}
