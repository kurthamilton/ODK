using ODK.Core.Venues;

namespace ODK.Services.Venues;

public interface IVenueAdminService
{
    Task<ServiceResult> CreateVenue(Guid currentMemberId, CreateVenue venue);

    Task<Venue> GetVenue(Guid currentMemberId, Guid venueId);

    Task<IReadOnlyCollection<Venue>> GetVenues(Guid currentMemberId, Guid chapterId);

    Task<IReadOnlyCollection<Venue>> GetVenues(Guid currentMemberId, Guid chapterId, IReadOnlyCollection<Guid> venueIds);

    Task<ServiceResult> UpdateVenue(Guid memberId, Guid id, CreateVenue venue);
}
