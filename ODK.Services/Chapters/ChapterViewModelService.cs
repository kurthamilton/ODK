using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Exceptions;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Chapters.ViewModels;

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

    public async Task<GroupsViewModel> FindGroups(ILocation location, Distance radius)
    {
        var chapters = await _unitOfWork.ChapterRepository.GetAll().RunAsync();
        var chapterLocations = await _unitOfWork.ChapterLocationRepository.GetAll();

        var chapterLocationDictionary = chapterLocations
            .ToDictionary(x => x.ChapterId);

        var groups = new List<ChapterWithDistanceViewModel>();
        foreach (var chapter in chapters)
        {
            if (!chapter.IsOpenForRegistration())
            {
                continue;
            }

            if (!chapterLocationDictionary.TryGetValue(chapter.Id, out var chapterLocation))
            {
                continue;
            }

            var distance = chapterLocation.LatLong.DistanceFrom(location.LatLong, radius.Unit);
            if (distance > radius.Value)
            {
                continue;
            }

            groups.Add(new ChapterWithDistanceViewModel
            {
                Chapter = chapter,
                Distance = distance,
                Location = chapterLocation
            });
        }

        return new GroupsViewModel
        {
            Distance = radius,
            Location = location,
            Groups = groups
                .OrderBy(x => x.Distance)
                .ToArray()
        };
    }

    public async Task<GroupsViewModel> FindGroups(Guid currentMemberId, Distance radius)
    {
        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMemberId);
        return await FindGroups(memberLocation, radius);
    }


    public async Task<ChapterCreateViewModel> GetChapterCreate(Guid currentMemberId)
    {
        var platform = _platformProvider.GetPlatform();
        if (platform != PlatformType.Default)
        {
            throw new OdkNotFoundException();
        }

        var (current, member, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByOwnerId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMemberId));

        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMemberId);

        return new ChapterCreateViewModel
        {
            ChapterCount = current.Count,
            ChapterLimit = memberSubscription.SiteSubscription.GroupLimit,
            Member = member,
            MemberLocation = memberLocation
        };
    }

    public async Task<GroupHomePageViewModel> GetGroupHomePage(Guid? currentMemberId, string slug)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).RunAsync();
        OdkAssertions.Exists(chapter);

        var (currentMember, adminMembers, memberCount, instagramPosts, links, questions, upcomingEvents, recentEvents) = await _unitOfWork.RunAsync(
            x => currentMemberId != null 
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null 
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQueryMultiple<ChapterAdminMember>(),
            x => x.MemberRepository.GetCountByChapterId(chapter.Id),
            x => x.InstagramPostRepository.GetByChapterId(chapter.Id, 8),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetByChapterId(chapter.Id, after: DateTime.UtcNow),
            x => x.EventRepository.GetRecentEventsByChapterId(chapter.Id, 3));

        var location = await _unitOfWork.ChapterLocationRepository.GetByChapterId(chapter.Id);

        var eventIds = upcomingEvents
            .Concat(recentEvents)
            .Select(x => x.Id)
            .Distinct()
            .ToArray();

        var (venues, responses) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByEventIds(eventIds),
            x => currentMember?.IsMemberOf(chapter.Id) == true
                ? x.EventResponseRepository.GetByMemberId(currentMember.Id, eventIds)
                : new DefaultDeferredQueryMultiple<EventResponse>());

        var venueDictionary = venues.ToDictionary(x => x.Id);
        var responseDictionary = responses.ToDictionary(x => x.EventId);

        return new GroupHomePageViewModel
        {
            Chapter = chapter,
            ChapterLocation = location,
            CurrentMember = currentMember,
            InstagramPosts = instagramPosts,
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            Links = links,
            MemberCount = memberCount,
            Questions = questions.OrderBy(x => x.DisplayOrder).ToArray(),
            RecentEvents = recentEvents
                .OrderByDescending(x => x.Date)
                .Select(x => new GroupHomePageEventViewModel
                {
                    Event = x,
                    Response = responseDictionary.ContainsKey(x.Id) ? responseDictionary[x.Id] : null,
                    Venue = venueDictionary[x.VenueId]
                })
                .ToArray(),
            UpcomingEvents = upcomingEvents
                .OrderBy(x => x.Date)
                .Select(x => new GroupHomePageEventViewModel
                {
                    Event = x,
                    Response = responseDictionary.ContainsKey(x.Id) ? responseDictionary[x.Id] : null,
                    Venue = venueDictionary[x.VenueId]
                })
                .ToArray()
        };
    }
    
    public async Task<ChapterHomePageViewModel> GetHomePage(Guid? currentMemberId, string chapterName)
    {        
        var chapter = await GetChapter(chapterName);
        OdkAssertions.MeetsCondition(chapter, x => x.IsOpenForRegistration());

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
