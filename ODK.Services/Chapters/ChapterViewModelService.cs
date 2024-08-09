using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Exceptions;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Platforms;

namespace ODK.Services.Chapters;

public class ChapterViewModelService : IChapterViewModelService
{
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterViewModelService(IUnitOfWork unitOfWork,
        IPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChapterCreateViewModel> GetChapterCreate(Guid currentMemberId)
    {
        var platform = _platformProvider.GetPlatform();
        if (platform != PlatformType.Default)
        {
            throw new OdkNotFoundException();
        }

        var (current, memberSubscription, countries) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByOwnerId(currentMemberId),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMemberId),
            x => x.CountryRepository.GetAll());

        return new ChapterCreateViewModel
        {
            ChapterCount = current.Count,
            ChapterLimit = memberSubscription.SiteSubscription.GroupLimit,
            Countries = countries
        };
    }

    public async Task<GroupHomePageViewModel> GetGroupHomePage(Guid? currentMemberId, string slug)
    {
        var (currentMember, chapter) = await _unitOfWork.RunAsync(
            x => currentMemberId != null 
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => x.ChapterRepository.GetBySlug(slug));

        return new GroupHomePageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember
        };
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

    public async Task<MemberChaptersViewModel> GetMemberChapters(Guid memberId)
    {
        var (chapters, adminMembers) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByMemberId(memberId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(memberId));

        var adminMemberDictionary = adminMembers
            .ToDictionary(x => x.ChapterId);

        var admin = new List<Chapter>();
        var member = new List<Chapter>();
        var owned = new List<Chapter>();

        foreach (var chapter in chapters)
        {
            if (chapter.OwnerId == memberId)
            {
                owned.Add(chapter);
            }
            else if (adminMemberDictionary.ContainsKey(chapter.Id))
            {
                admin.Add(chapter);
            }
            else
            {
                member.Add(chapter);
            }
        }

        return new MemberChaptersViewModel
        {
            Admin = admin,
            Member = member,
            Owned = owned
        };
    }

    private async Task<Chapter> GetChapter(string name)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(name).RunAsync();
        return OdkAssertions.Exists(chapter);        
    }
}
