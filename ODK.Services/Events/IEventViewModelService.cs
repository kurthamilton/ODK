using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventViewModelService
{
    Task<EventPageViewModel> GetEventPageViewModel(Guid? currentMemberId, string chapterName, Guid eventId);

    Task<EventsPageViewModel> GetEventsPage(Guid? currentMemberId, string chapterName);

    Task<EventPageViewModel> GetGroupEventPageViewModel(Guid? currentMemberId, string slug, Guid eventId);    
}
