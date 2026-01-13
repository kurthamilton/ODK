using ODK.Core.Chapters;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventViewModelService
{
    Task<EventCheckoutPageViewModel> GetEventCheckoutPageViewModel
        (ServiceRequest request, Guid currentMemberId, Chapter chapter, Guid eventId);

    Task<EventPageViewModel> GetEventPageViewModel(
        ServiceRequest request, Guid? currentMemberId, Chapter chapter, Guid eventId);

    Task<EventsPageViewModel> GetEventsPage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);
}
