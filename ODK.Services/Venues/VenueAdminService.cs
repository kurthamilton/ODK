using ODK.Core;
using ODK.Core.Countries;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Caching;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public class VenueAdminService : OdkAdminServiceBase, IVenueAdminService
{
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public VenueAdminService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
        : base(unitOfWork)
    {
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CreateVenue(MemberChapterServiceRequest request, CreateVenue model)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var existing = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetByName(chapterId, model.Name));

        var venue = new Venue
        {
            Address = model.Address,
            ChapterId = chapterId,
            MapQuery = model.LocationName,
            Name = model.Name
        };

        var location = new VenueLocation
        {
            LatLong = model.Location ?? new LatLong(),
            Name = model.LocationName ?? string.Empty
        };

        var validationResult = ValidateVenue(venue, existing, location);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.VenueRepository.Add(venue);

        location.VenueId = venue.Id;
        _unitOfWork.VenueLocationRepository.Add(location);

        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedCollection<Venue>(request.ChapterId);

        return ServiceResult.Successful();
    }

    public async Task<Venue> GetVenue(MemberChapterServiceRequest request, Guid venueId)
    {
        var venue = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetById(venueId));
        return OdkAssertions.BelongsToChapter(venue, request.ChapterId);
    }

    public async Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(
        MemberChapterServiceRequest request, Guid venueId)
    {
        var platform = request.Platform;

        var (chapter, venue, events) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.VenueRepository.GetById(venueId),
            x => x.EventRepository.GetByVenueId(venueId));

        OdkAssertions.BelongsToChapter(venue, request.ChapterId);

        return new VenueEventsAdminPageViewModel
        {
            Chapter = chapter,
            Events = events,
            Platform = platform,
            Venue = venue
        };
    }

    public async Task<VenuesAdminPageViewModel> GetVenuesViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, venues, events) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.VenueRepository.GetByChapterId(request.ChapterId),
            x => x.EventRepository.GetByChapterId(request.ChapterId));

        return new VenuesAdminPageViewModel
        {
            Chapter = chapter,
            Events = events,
            Platform = platform,
            Venues = venues
        };
    }

    public async Task<VenueAdminPageViewModel> GetVenueViewModel(MemberChapterServiceRequest request, Guid venueId)
    {
        var platform = request.Platform;

        var (chapter, venue) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.VenueRepository.GetById(venueId));

        OdkAssertions.BelongsToChapter(venue, request.ChapterId);

        var location = await _unitOfWork.VenueLocationRepository.GetByVenueId(venueId);

        return new VenueAdminPageViewModel
        {
            Chapter = chapter,
            Location = location,
            Platform = platform,
            Venue = venue
        };
    }

    public async Task<ServiceResult> UpdateVenue(MemberChapterServiceRequest request, Guid id, CreateVenue model)
    {
        var (venue, existing) = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetById(id),
            x => x.VenueRepository.GetByName(request.ChapterId, model.Name));

        var location = await _unitOfWork.VenueLocationRepository.GetByVenueId(id);

        OdkAssertions.BelongsToChapter(venue, request.ChapterId);

        venue.Address = model.Address;
        venue.MapQuery = model.LocationName;
        venue.Name = model.Name;

        location ??= new VenueLocation();

        location.Name = model.LocationName ?? string.Empty;
        location.LatLong = model.Location ?? new LatLong();

        var validationResult = ValidateVenue(venue, existing, location);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.VenueRepository.Update(venue);

        if (location.VenueId == default)
        {
            location.VenueId = venue.Id;
            _unitOfWork.VenueLocationRepository.Add(location);
        }
        else
        {
            _unitOfWork.VenueLocationRepository.Update(location);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<Venue>(id);
        _cacheService.RemoveVersionedCollection<Venue>(venue.ChapterId);

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateVenue(Venue venue, Venue? existing, VenueLocation location)
    {
        if (string.IsNullOrWhiteSpace(venue.Name))
        {
            return ServiceResult.Failure("Name required");
        }

        if (existing != null && existing.Id != venue.Id)
        {
            return ServiceResult.Failure("Venue with that name already exists");
        }

        if (string.IsNullOrEmpty(location.Name) || location.LatLong.IsDefault)
        {
            return ServiceResult.Failure("Location not set");
        }

        return ServiceResult.Successful();
    }
}
