using ODK.Core;
using ODK.Core.Countries;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Caching;
using ODK.Services.Venues.Models;
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

    public async Task<ServiceResult> CreateVenue(
        IMemberChapterAdminServiceRequest request, VenueCreateModel model)
    {
        var chapter = request.Chapter;

        var existing = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetByName(chapter.Id, model.Name));

        var venue = new Venue
        {
            Address = model.Address,
            ChapterId = chapter.Id,
            MapQuery = model.LocationName,
            Name = model.Name
        };

        var location = new VenueLocation
        {
            Latitude = (decimal?)model.Location?.Lat ?? 0,
            Longitude = (decimal?)model.Location?.Long ?? 0,
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

        _cacheService.RemoveVersionedCollection<Venue>(chapter.Id);

        return ServiceResult.Successful();
    }

    public async Task<Venue> GetVenue(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var chapter = request.Chapter;

        var venue = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId));

        return OdkAssertions.BelongsToChapter(venue, chapter.Id);
    }

    public async Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (venue, events) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId),
            x => x.EventRepository.GetByVenueId(venueId));

        OdkAssertions.BelongsToChapter(venue, chapter.Id);

        return new VenueEventsAdminPageViewModel
        {
            Chapter = chapter,
            Events = events,
            Platform = platform,
            Venue = venue
        };
    }

    public async Task<VenuesAdminPageViewModel> GetVenuesViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (venues, events) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetByChapterId(chapter.Id));

        return new VenuesAdminPageViewModel
        {
            Chapter = chapter,
            Events = events,
            Platform = platform,
            Venues = venues
        };
    }

    public async Task<VenueAdminPageViewModel> GetVenueViewModel(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (venue, location) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId),
            x => x.VenueLocationRepository.GetByVenueId(venueId));

        OdkAssertions.BelongsToChapter(venue, chapter.Id);

        return new VenueAdminPageViewModel
        {
            Chapter = chapter,
            Location = location,
            Platform = platform,
            Venue = venue
        };
    }

    public async Task<ServiceResult> UpdateVenue(
        IMemberChapterAdminServiceRequest request, Guid id, VenueCreateModel model)
    {
        var chapter = request.Chapter;

        var (venue, existing, location) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(id),
            x => x.VenueRepository.GetByName(chapter.Id, model.Name),
            x => x.VenueLocationRepository.GetByVenueId(id));

        OdkAssertions.BelongsToChapter(venue, chapter.Id);

        venue.Address = model.Address;
        venue.MapQuery = model.LocationName;
        venue.Name = model.Name;

        location ??= new VenueLocation();

        location.Name = model.LocationName ?? string.Empty;
        location.Latitude = (decimal?)model.Location?.Lat ?? 0;
        location.Longitude = (decimal?)model.Location?.Long ?? 0;

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