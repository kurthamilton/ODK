using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventViewModelService
{
    Task<EventCheckoutPageViewModel> GetEventCheckoutPageViewModel
        (MemberChapterServiceRequest request, string shortcode, string returnPath);

    Task<EventPageViewModel> GetEventPageViewModel(
        ChapterServiceRequest request, Member? currentMember, string shortcode);

    Task<EventsPageViewModel> GetEventsPage(ChapterServiceRequest request, Guid? currentMemberId);
}