using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Caching;

namespace ODK.Services.Venues;

public class VenueAdminService : OdkAdminServiceBase, IVenueAdminService
{
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public VenueAdminService(IUnitOfWork unitOfWork,
        ICacheService cacheService)
        : base(unitOfWork)
    {
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CreateVenue(AdminServiceRequest request, CreateVenue model)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var existing = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetByName(chapterId, model.Name));

        var venue = new Venue
        {
            Address = model.Address,
            ChapterId = chapterId,
            MapQuery = model.MapQuery,
            Name = model.Name            
        };

        var validationResult = ValidateVenue(venue, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.VenueRepository.Add(venue);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedCollection<Venue>(request.ChapterId);

        return ServiceResult.Successful();
    }
    
    public async Task<Venue> GetVenue(AdminServiceRequest request, Guid venueId)
    {
        var venue = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetById(venueId));
        AssertBelongsToChapter(venue, request);
        return venue;
    }

    public async Task<IReadOnlyCollection<Venue>> GetVenues(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<VenuesDto> GetVenuesDto(AdminServiceRequest request)
    {
        var (venues, events) = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetByChapterId(request.ChapterId),
            x => x.EventRepository.GetByChapterId(request.ChapterId));

        return new VenuesDto
        {
            Events = events,
            Venues = venues
        };
    }

    public async Task<ServiceResult> UpdateVenue(AdminServiceRequest request, Guid id, CreateVenue model)
    {       
        var (venue, existing) = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetById(id),
            x => x.VenueRepository.GetByName(request.ChapterId, model.Name));

        AssertBelongsToChapter(venue, request);

        venue.Address = model.Address;
        venue.MapQuery = model.MapQuery;
        venue.Name = model.Name;
        
        var validationResult = ValidateVenue(venue, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.VenueRepository.Update(venue);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<Venue>(id);
        _cacheService.RemoveVersionedCollection<Venue>(venue.ChapterId);

        return ServiceResult.Successful();
    }
    
    private ServiceResult ValidateVenue(Venue venue, Venue? existing)
    {
        if (string.IsNullOrWhiteSpace(venue.Name))
        {
            return ServiceResult.Failure("Name required");
        }

        if (existing != null && existing.Id != venue.Id)
        {
            return ServiceResult.Failure("Venue with that name already exists");
        }

        return ServiceResult.Successful();
    }
}
