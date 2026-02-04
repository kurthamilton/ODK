using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventViewModelService
{
    Task<EventCheckoutPageViewModel> GetEventCheckoutPageViewModel
        (MemberChapterServiceRequest request, string shortcode, string returnPath);

    Task<EventPageViewModel> GetEventPageViewModel(
        ServiceRequest request, Member? currentMember, Chapter chapter, string shortcode);

    Task<EventsPageViewModel> GetEventsPage(ServiceRequest request, Guid? currentMemberId, Chapter chapter);
}