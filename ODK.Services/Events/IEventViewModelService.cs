using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventViewModelService
{
    Task<EventPageViewModel> GetEventPageViewModel(
        ServiceRequest request, Guid? currentMemberId, string chapterName, Guid eventId);

    Task<EventsPageViewModel> GetEventsPage(ServiceRequest request, Guid? currentMemberId, string chapterName);

    Task<EventPageViewModel> GetGroupEventPageViewModel(
        ServiceRequest request, Guid? currentMemberId, string slug, Guid eventId);    
}
