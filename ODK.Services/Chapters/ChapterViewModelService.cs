using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public class ChapterViewModelService : IChapterViewModelService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChapterViewModelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChapterHomePageViewModel> GetHomePage(Guid? currentMemberId, string chapterName)
    {
        var chapter = await GetChapter(chapterName);
        var today = chapter.TodayUtc();

        var (currentMember, events, links, texts, instagramPosts, latestMembers) = await _unitOfWork.RunAsync(
            x => currentMemberId != null 
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) 
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => x.EventRepository.GetByChapterId(chapter.Id, today),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
            x => x.InstagramPostRepository.GetByChapterId(chapter.Id, 8),
            x => x.MemberRepository.GetLatestByChapterId(chapter.Id, 8));

        events = events
            .Where(x => x.IsAuthorized(currentMember) && x.IsPublished)
            .ToArray();

        var eventIds = events.Select(x => x.Id).ToArray();

        var (venues, responses) = await _unitOfWork.RunAsync(
            x => eventIds.Any() 
                ? x.VenueRepository.GetByChapterId(chapter.Id, eventIds) 
                : new DefaultDeferredQueryMultiple<Venue>(),
            x => eventIds.Any() && currentMemberId != null 
                ? x.EventResponseRepository.GetByMemberId(currentMemberId.Value, eventIds) 
                : new DefaultDeferredQueryMultiple<EventResponse>());

        return new ChapterHomePageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            Events = events,
            EventVenues = venues,
            InstagramPosts = instagramPosts,
            LatestMembers = latestMembers,
            Links = links,
            MemberEventResponses = responses,
            Texts = texts
        };
    }

    private async Task<Chapter> GetChapter(string name)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(name).RunAsync();
        return OdkAssertions.Exists(chapter);        
    }
}
