using ODK.Core.Venues;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public interface IVenueAdminService
{
    Task<ServiceResult> CreateVenue(MemberChapterServiceRequest request, CreateVenue venue);

    Task<Venue> GetVenue(MemberChapterServiceRequest request, Guid venueId);

    Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(MemberChapterServiceRequest request, Guid venueId);

    Task<VenuesAdminPageViewModel> GetVenuesViewModel(MemberChapterServiceRequest request);

    Task<VenueAdminPageViewModel> GetVenueViewModel(MemberChapterServiceRequest request, Guid venueId);

    Task<ServiceResult> UpdateVenue(MemberChapterServiceRequest request, Guid id, CreateVenue venue);
}
