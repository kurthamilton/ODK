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

    public async Task<ServiceResult> CreateVenue(Guid currentMemberId, CreateVenue model)
    {
        var (chapterAdminMembers, currentMember, existing) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(model.ChapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.VenueRepository.GetByName(model.ChapterId, model.Name));

        AssertMemberIsChapterAdmin(currentMember, model.ChapterId, chapterAdminMembers);

        var venue = new Venue
        {
            Address = model.Address,
            ChapterId = model.ChapterId,
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

        _cacheService.RemoveVersionedCollection<Venue>(model.ChapterId);

        return ServiceResult.Successful();
    }
    
    public async Task<Venue> GetVenue(Guid currentMemberId, Guid venueId)
    {
        var venue = await _unitOfWork.VenueRepository.GetById(venueId).RunAsync();
        await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);
        return venue;
    }

    public async Task<IReadOnlyCollection<Venue>> GetVenues(Guid currentMemberId, Guid chapterId)
    {
        var (chapterAdminMembers, currentMember, venues) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.VenueRepository.GetByChapterId(chapterId));
        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);
        return venues;
    }

    public async Task<VenuesDto> GetVenuesDto(Guid currentMemberId, Guid chapterId)
    {
        var (chapterAdminMembers, currentMember, venues, events) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.VenueRepository.GetByChapterId(chapterId),
            x => x.EventRepository.GetByChapterId(chapterId));
        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);
        return new VenuesDto
        {
            Events = events,
            Venues = venues
        };
    }

    public async Task<ServiceResult> UpdateVenue(Guid currentMemberId, Guid id, CreateVenue model)
    {       
        var (chapterAdminMembers, currentMember, venue, existing) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(model.ChapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.VenueRepository.GetById(id),
            x => x.VenueRepository.GetByName(model.ChapterId, model.Name));

        AssertMemberIsChapterAdmin(currentMember, model.ChapterId, chapterAdminMembers);

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
