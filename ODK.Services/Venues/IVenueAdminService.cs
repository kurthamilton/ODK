using ODK.Core.Venues;
using ODK.Services.Venues.Models;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public interface IVenueAdminService
{
    Task<ServiceResult> ArchiveVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId);

    Task<ServiceResult> CreateVenue(IMemberChapterAdminServiceRequest request, VenueCreateModel venue);

    Task<ChapterVenue> GetChapterVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId);

    Task<VenueCreateAdminPageViewModel> GetVenueCreateViewModel(IMemberChapterAdminServiceRequest request);

    Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(IMemberChapterAdminServiceRequest request, Guid chapterVenueId);

    Task<VenuesAdminPageViewModel> GetVenuesViewModel(IMemberChapterAdminServiceRequest request, bool archived);

    Task<VenueAdminPageViewModel> GetVenueViewModel(IMemberChapterAdminServiceRequest request, Guid chapterVenueId);

    Task<ServiceResult> RemoveVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId);

    Task<ServiceResult> RestoreVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId);

    Task<ServiceResult> UpdateVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId, VenueUpdateModel venue);
}