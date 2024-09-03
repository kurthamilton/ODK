using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Exceptions;
using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public class ChapterViewModelService : IChapterViewModelService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterViewModelService(
        IUnitOfWork unitOfWork,
        IPlatformProvider platformProvider,
        IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<GroupsViewModel> FindGroups(ILocation location, Distance radius)
    {
        var platform = _platformProvider.GetPlatform();

        var chapters = await _unitOfWork.ChapterRepository.GetAll().Run();
        var chapterLocations = await _unitOfWork.ChapterLocationRepository.GetAll();

        var chapterDictionary = chapters
            .ToDictionary(x => x.Id);
        var chapterLocationDictionary = chapterLocations
            .ToDictionary(x => x.ChapterId);

        var chapterDistances = new Dictionary<Guid, double>();

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

            chapterDistances.Add(chapter.Id, distance);
        }

        var chapterIds = chapterDistances.Keys.ToArray();

        var texts = chapterIds.Length > 0
            ? await _unitOfWork.ChapterTextsRepository.GetByChapterIds(chapterIds).Run()
            : [];

        var textsDictionary = texts
            .ToDictionary(x => x.ChapterId);

        var groups = new List<ChapterWithDistanceViewModel>();
        foreach (var chapterId in chapterIds)
        {
            textsDictionary.TryGetValue(chapterId, out var chapterTexts);

            groups.Add(new ChapterWithDistanceViewModel
            {
                Chapter = chapterDictionary[chapterId],
                Distance = chapterDistances[chapterId],
                Location = chapterLocationDictionary[chapterId],
                Platform = platform,
                Texts = chapterTexts
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
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMemberId, platform));

        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMemberId);

        return new ChapterCreateViewModel
        {
            ChapterCount = current.Count,
            ChapterLimit = memberSubscription.SiteSubscription.GroupLimit,
            Member = member,
            MemberLocation = memberLocation,
            Platform = platform
        };
    }

    public async Task<GroupContactPageViewModel> GetGroupContactPage(Guid? currentMemberId, string slug)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (
            currentMember,
            ownerSubscription,
            adminMembers,
            hasQuestions
        ) = await _unitOfWork.RunAsync(
            x => currentMemberId != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQueryMultiple<ChapterAdminMember>(),       
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id));

        return new GroupContactPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<GroupEventsPageViewModel> GetGroupEventsPage(Guid? currentMemberId, string slug)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (
            currentMember,
            memberSubscription,
            ownerSubscription,
            membershipSettings,
            privacySettings,
            adminMembers,
            upcomingEvents,
            hasQuestions
        ) = await _unitOfWork.RunAsync(
            x => currentMemberId != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQueryMultiple<ChapterAdminMember>(),
            x => x.EventRepository.GetByChapterId(chapter.Id, after: DateTime.UtcNow),            
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id));

        var eventIds = upcomingEvents
            .Select(x => x.Id)
            .Distinct()
            .ToArray();

        var (venues, responses) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByEventIds(eventIds),
            x => currentMember?.IsMemberOf(chapter.Id) == true
                ? x.EventResponseRepository.GetByMemberId(currentMember.Id, eventIds)
                : new DefaultDeferredQueryMultiple<EventResponse>());

        return new GroupEventsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Events = ToGroupPageListEvents(
                upcomingEvents,
                venues,
                responses,
                currentMember,
                memberSubscription,
                membershipSettings,
                privacySettings),
            Platform = platform          
        };
    }

    public async Task<GroupEventsPageViewModel> GetGroupPastEventsPage(Guid? currentMemberId, string slug)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (
            currentMember,
            memberSubscription,
            ownerSubscription,
            membershipSettings,
            privacySettings,
            adminMembers,
            pastEvents,
            hasQuestions
        ) = await _unitOfWork.RunAsync(
            x => currentMemberId != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQueryMultiple<ChapterAdminMember>(),
            x => x.EventRepository.GetRecentEventsByChapterId(chapter.Id, 1000),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id));

        var eventIds = pastEvents
            .Select(x => x.Id)
            .Distinct()
            .ToArray();

        var (venues, responses) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByEventIds(eventIds),
            x => currentMember?.IsMemberOf(chapter.Id) == true
                ? x.EventResponseRepository.GetByMemberId(currentMember.Id, eventIds)
                : new DefaultDeferredQueryMultiple<EventResponse>());

        return new GroupEventsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Events = ToGroupPageListEvents(
                pastEvents,
                venues,
                responses,
                currentMember,
                memberSubscription,
                membershipSettings,
                privacySettings),
            Platform = platform
        };
    }

    public async Task<GroupHomePageViewModel> GetGroupHomePage(Guid? currentMemberId, string slug)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (
            currentMember, 
            memberSubscription, 
            ownerSubscription,
            membershipSettings,
            privacySettings,
            adminMembers, 
            memberCount, 
            instagramPosts, 
            links, 
            upcomingEvents, 
            recentEvents, 
            hasQuestions, 
            texts
        ) = await _unitOfWork.RunAsync(
            x => currentMemberId != null 
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),            
            x => currentMemberId != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null 
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQueryMultiple<ChapterAdminMember>(),
            x => x.MemberRepository.GetCountByChapterId(chapter.Id),
            x => x.InstagramPostRepository.GetByChapterId(chapter.Id, 8),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetByChapterId(chapter.Id, after: DateTime.UtcNow),
            x => x.EventRepository.GetRecentEventsByChapterId(chapter.Id, 3),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id));

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

        var recentEventViewModels = ToGroupPageListEvents(
            recentEvents.OrderByDescending(x => x.Date),
            venues,
            responses,
            currentMember,
            memberSubscription,
            membershipSettings,
            privacySettings);

        var upcomingEventViewModels = ToGroupPageListEvents(
            upcomingEvents.OrderBy(x => x.Date),
            venues,
            responses,
            currentMember,
            memberSubscription,
            membershipSettings,
            privacySettings);

        return new GroupHomePageViewModel
        {
            Chapter = chapter,
            ChapterLocation = location,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            InstagramPosts = ownerSubscription?.HasFeature(SiteFeatureType.InstagramFeed) == true
                ? instagramPosts
                : [],
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            Links = links,
            MemberCount = memberCount,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            RecentEvents = recentEventViewModels,
            Texts = texts,
            UpcomingEvents = upcomingEventViewModels
        };
    }
    
    public async Task<GroupJoinPageViewModel> GetGroupJoinPage(Guid? currentMemberId, string slug)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (currentMember, adminMembers, ownerSubscription, hasQuestions, properties, propertyOptions, texts, membershipSettings) = await _unitOfWork.RunAsync(
            x => currentMemberId != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQueryMultiple<ChapterAdminMember>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

        return new GroupJoinPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            MembershipSettings = membershipSettings,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Properties = properties,
            PropertyOptions = propertyOptions,
            RegistrationOpen = chapter.IsOpenForRegistration(),
            Texts = texts
        };
    }

    public async Task<GroupProfilePageViewModel> GetGroupProfilePage(Guid currentMemberId, string slug)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (
            currentMember, 
            adminMembers, 
            ownerSubscription,
            hasQuestions, 
            chapterProperties, 
            chapterPropertyOptions,
            memberProperties
        ) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(currentMemberId, chapter.Id));

        return new GroupProfilePageViewModel
        {
            Chapter = chapter,
            ChapterProperties = chapterProperties,
            ChapterPropertyOptions = chapterPropertyOptions,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            IsMember = currentMember.IsMemberOf(chapter.Id) == true,
            MemberProperties = memberProperties,
            OwnerSubscription = ownerSubscription,
            Platform = platform            
        };
    }

    public async Task<GroupProfileSubscriptionPageViewModel> GetGroupProfileSubscriptionPage(Guid currentMemberId, string slug)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (
            currentMember,
            adminMembers,
            ownerSubscription,
            hasQuestions,
            chapterPaymentSettings,
            sitePaymentSettings
        ) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.SitePaymentSettingsRepository.Get());

        return new GroupProfileSubscriptionPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            IsMember = currentMember.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            ChapterPaymentSettings = chapterPaymentSettings,
            Platform = platform,
            SitePaymentSettings = sitePaymentSettings
        };
    }

    public async Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(Guid? currentMemberId, string slug)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (currentMember, adminMembers, ownerSubscription, questions) = await _unitOfWork.RunAsync(
            x => currentMemberId != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQueryMultiple<ChapterAdminMember>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.GetByChapterId(chapter.Id));

        return new GroupQuestionsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = questions.Count > 0,
            IsAdmin = adminMembers.Any(x => x.ChapterId == chapter.Id),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Questions = questions.OrderBy(x => x.DisplayOrder).ToArray()
        };
    }

    public async Task<ChapterHomePageViewModel> GetHomePage(Guid? currentMemberId, string chapterName)
    {        
        var platform = _platformProvider.GetPlatform();

        var chapter = await GetChapter(chapterName);
        OdkAssertions.MeetsCondition(chapter, x => x.IsOpenForRegistration());

        var today = chapter.TodayUtc();

        var (currentMember, memberSubscription, membershipSettings, privacySettings, events, links, texts, instagramPosts, latestMembers) = await _unitOfWork.RunAsync(
            x => currentMemberId != null 
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) 
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetByChapterId(chapter.Id, today),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
            x => x.InstagramPostRepository.GetByChapterId(chapter.Id, 8),
            x => x.MemberRepository.GetLatestByChapterId(chapter.Id, 8));

        var eventIds = events.Select(x => x.Id).ToArray();

        var (venues, responses) = await _unitOfWork.RunAsync(
            x => eventIds.Any() 
                ? x.VenueRepository.GetByEventIds(eventIds)
                : new DefaultDeferredQueryMultiple<Venue>(),
            x => eventIds.Any() && currentMemberId != null 
                ? x.EventResponseRepository.GetByMemberId(currentMemberId.Value, eventIds) 
                : new DefaultDeferredQueryMultiple<EventResponse>());

        return new ChapterHomePageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            Events = events
                .Where(x => _authorizationService.CanViewEvent(x, currentMember, memberSubscription, membershipSettings, privacySettings))
                .OrderBy(x => x.Date)
                .ToArray(),
            EventVenues = venues
                .Where(x => _authorizationService.CanViewVenue(x, currentMember, memberSubscription, membershipSettings, privacySettings))
                .ToArray(),
            InstagramPosts = instagramPosts,
            LatestMembers = latestMembers,
            Links = links,
            MemberEventResponses = responses,
            Platform = platform,
            Texts = texts
        };
    }

    public async Task<MemberChaptersViewModel> GetMemberChapters(Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapters, adminMembers) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByMemberId(memberId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(memberId));

        var chapterIds = chapters
            .Select(x => x.Id)
            .ToArray();

        var texts = chapterIds.Length > 0
            ? await _unitOfWork.ChapterTextsRepository.GetByChapterIds(chapterIds).Run()
            : [];

        var adminMemberDictionary = adminMembers
            .ToDictionary(x => x.ChapterId);
        var textsDictionary = texts
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
            Owned = owned,
            Platform = platform,
            Texts = texts
        };
    }

    private async Task<Chapter> GetChapter(string name)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(name).Run();
        return OdkAssertions.Exists(chapter);        
    }

    private IReadOnlyCollection<GroupPageListEventViewModel> ToGroupPageListEvents(
        IEnumerable<Event> events, 
        IEnumerable<Venue> venues,
        IEnumerable<EventResponse> responses,
        Member? currentMember,
        MemberSubscription? memberSubscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        var viewModels = new List<GroupPageListEventViewModel>();

        var venueDictionary = venues.ToDictionary(x => x.Id);
        var responseDictionary = responses.ToDictionary(x => x.EventId);

        foreach (var @event in events)
        {
            var canViewEvent = _authorizationService.CanViewEvent(@event, currentMember, memberSubscription, membershipSettings, privacySettings);
            if (!canViewEvent)
            {
                continue;
            }

            var venue = venueDictionary[@event.VenueId];
            var canViewVenue = _authorizationService.CanViewVenue(venue, currentMember, memberSubscription, membershipSettings, privacySettings);
            viewModels.Add(new GroupPageListEventViewModel
            {
                Event = @event,
                Response = responseDictionary.ContainsKey(@event.Id) ? responseDictionary[@event.Id] : null,
                Venue = canViewVenue ? venue : null
            });
        }     
        
        return viewModels;
    }
}
