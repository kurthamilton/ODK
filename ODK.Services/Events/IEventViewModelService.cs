using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventViewModelService
{
    Task<EventCheckoutPageViewModel> GetEventCheckoutPageViewModel(
        IMemberChapterServiceRequest request, string shortcode, string returnPath);

    Task<EventPageViewModel> GetEventPageViewModel(
        IChapterServiceRequest request, string shortcode);

    Task<EventsPageViewModel> GetEventsPage(IChapterServiceRequest request);
}