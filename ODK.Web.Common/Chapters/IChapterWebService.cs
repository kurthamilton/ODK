using System;
using System.Threading.Tasks;

namespace ODK.Web.Common.Chapters;
public interface IChapterWebService
{
    Task<AboutPageViewModel> GetAboutPageViewModelAsync(Guid? currentMemberId, string chapterName);

    Task<ChapterPageViewModel> GetChapterPageViewModelAsync(Guid? currentMemberId, string chapterName);

    Task<ContactPageViewModel> GetContactPageViewModelAsync(Guid? currentMemberId, string chapterName);

    Task<EventPageViewModel> GetEventPageViewModelAsync(Guid? currentMemberId, string chapterName, Guid eventId);

    Task<EventsPageViewModel> GetEventsPageViewModelAsync(Guid? currentMemberId, string chapterName);
}
