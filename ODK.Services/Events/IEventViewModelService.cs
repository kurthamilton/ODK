using ODK.Core.Members;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventViewModelService
{
    Task<EventCheckoutPageViewModel> GetEventCheckoutPageViewModel(
        IMemberChapterServiceRequest request, string shortcode, string returnPath);

    Task<EventPageViewModel> GetEventPageViewModel(
        IChapterServiceRequest request, Member? currentMember, string shortcode);

    Task<EventsPageViewModel> GetEventsPage(IChapterServiceRequest request, Guid? currentMemberId);
}