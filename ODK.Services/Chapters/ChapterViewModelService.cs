using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Exceptions;
using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Events.ViewModels;

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

    public async Task<GroupsViewModel> FindGroups(Guid? currentMemberId, GroupFilter filter)
    {
        var platform = _platformProvider.GetPlatform();

        MemberLocation? memberLocation = null;

        if (filter.Location == null && currentMemberId != null)
        {
            memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMemberId.Value);
        }

        var topicGroups = await _unitOfWork.TopicGroupRepository.GetAll().Run();
        var topicGroup = topicGroups
            .FirstOrDefault(x => string.Equals(x.Name, filter.TopicGroup, StringComparison.InvariantCultureIgnoreCase));

        var (chapters, distanceUnits, preferences) = await _unitOfWork.RunAsync(
            x => topicGroup != null 
                ? x.ChapterRepository.GetByTopicGroupId(topicGroup.Id)
                : x.ChapterRepository.GetAll(),
            x => x.DistanceUnitRepository.GetAll(),
            x => currentMemberId != null
                ? x.MemberPreferencesRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<MemberPreferences>());

        // TODO: search by location in the database
        var chapterLocations = await _unitOfWork.ChapterLocationRepository.GetByChapterIds(chapters.Select(x => x.Id));
        
        distanceUnits = distanceUnits
            .OrderBy(x => x.Order)
            .ToArray();

        var distanceUnit = distanceUnits
            .FirstOrDefault(x => string.Equals(x.Abbreviation, filter.DistanceUnit, StringComparison.InvariantCultureIgnoreCase))
            ?? distanceUnits.FirstOrDefault(x => x.Id == preferences?.DistanceUnitId)
            ?? distanceUnits.First();

        var distance = filter.Distance ?? 30;

        var location = filter.Location != null && filter.LocationName != null
            ? new Location
            {
                LatLong = filter.Location.Value,
                Name = filter.LocationName
            }
            : memberLocation != null
                ? new Location
                {
                    LatLong = memberLocation.LatLong,
                    Name = memberLocation.Name
                }
                : null;

        var chaptersWithDistances = FilterChaptersByDistance(
            chapters, 
            chapterLocations, 
            location?.LatLong, 
            new Distance { Unit = distanceUnit, Value = distance });
        
        var texts = chapters.Count > 0
            ? await _unitOfWork.ChapterTextsRepository.GetByChapterIds(chapters.Select(x => x.Id)).Run()
            : [];

        var chapterDictionary = chapters
            .ToDictionary(x => x.Id);
        var chapterLocationDictionary = chapterLocations
            .ToDictionary(x => x.ChapterId);

        var textsDictionary = texts
            .ToDictionary(x => x.ChapterId);

        var groups = new List<ChapterWithDistanceViewModel>();
        foreach (var chapterId in chaptersWithDistances.Keys)
        {
            chapterLocationDictionary.TryGetValue(chapterId, out var chapterLocation);
            textsDictionary.TryGetValue(chapterId, out var chapterTexts);

            groups.Add(new ChapterWithDistanceViewModel
            {
                Chapter = chapterDictionary[chapterId],
                Distance = chaptersWithDistances[chapterId],
                Location = chapterLocationDictionary[chapterId],
                Platform = platform,
                Texts = chapterTexts
            });
        }

        return new GroupsViewModel
        {
            Distance = new Distance { Unit = distanceUnit, Value = distance },
            DistanceUnits = distanceUnits,
            Groups = groups,
            Location = location,
            Platform = platform,
            TopicGroupId = topicGroup?.Id,
            TopicGroups = topicGroups
        };
    }

    public async Task<ChapterCreateViewModel> GetChapterCreate(Guid currentMemberId)
    {
        var platform = _platformProvider.GetPlatform();
        if (platform != PlatformType.Default)
        {
            throw new OdkNotFoundException();
        }

        var (current, member, memberSubscription, topicGroups, topics, memberTopics) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByOwnerId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMemberId, platform),
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll(),
            x => x.MemberTopicRepository.GetByMemberId(currentMemberId));

        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMemberId);

        return new ChapterCreateViewModel
        {
            ChapterCount = current.Count,
            ChapterLimit = memberSubscription?.SiteSubscription.GroupLimit ?? 1,
            Member = member,
            MemberLocation = memberLocation,
            MemberTopics = memberTopics,
            Platform = platform,
            TopicGroups = topicGroups,
            Topics = topics
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
            hasQuestions,
            conversations,
            membershipSettings,
            privacySettings,
            memberSubscription
        ) = await _unitOfWork.RunAsync(
            x => currentMemberId != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => currentMemberId != null
                ? x.ChapterConversationRepository.GetDtosByMemberId(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQueryMultiple<ChapterConversationDto>(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>());

        var canStartConversation = currentMember != null
            ? _authorizationService.CanStartConversation(chapter.Id, currentMember, memberSubscription, membershipSettings, privacySettings)
            : false;

        return new GroupContactPageViewModel
        {
            CanStartConversation = canStartConversation,
            Chapter = chapter,
            Conversations = conversations,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<GroupConversationPageViewModel> GetGroupConversationPage(Guid currentMemberId, string slug, 
        Guid conversationId)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        var (
            currentMember,
            ownerSubscription,
            adminMembers,
            hasQuestions,
            conversations,
            messages,
            notifications
        ) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByIdOrDefault(currentMemberId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterConversationRepository.GetDtosByMemberId(currentMemberId, chapter.Id),
            x => x.ChapterConversationMessageRepository.GetByConversationId(conversationId),
            x => x.NotificationRepository.GetUnreadByMemberId(currentMemberId, NotificationType.ConversationAdminMessage, conversationId));

        var dto = conversations
            .FirstOrDefault(x => x.Conversation.Id == conversationId);
        OdkAssertions.Exists(dto);

        var unread = messages
            .Where(x => !x.ReadByMember)
            .ToArray();

        if (unread.Length > 0)
        {
            unread.ForEach(x => x.ReadByMember = true);
            _unitOfWork.ChapterConversationMessageRepository.UpdateMany(unread);            
        }

        if (notifications.Count > 0)
        {
            _unitOfWork.NotificationRepository.MarkAsRead(notifications);
        }

        if (unread.Length > 0 || notifications.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return new GroupConversationPageViewModel
        {
            Chapter = chapter,
            Conversation = dto.Conversation,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            Messages = messages,
            OwnerSubscription = ownerSubscription,
            OtherConversations = conversations
                .Where(x => x.Conversation.Id != conversationId)
                .ToArray(),
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
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetByChapterId(chapter.Id, after: DateTime.UtcNow),            
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id));

        var eventIds = upcomingEvents
            .Select(x => x.Id)
            .Distinct()
            .ToArray();

        var (venues, memberResponses, responseSummaries) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByEventIds(eventIds),
            x => currentMember?.IsMemberOf(chapter.Id) == true
                ? x.EventResponseRepository.GetByMemberId(currentMember.Id, eventIds)
                : new DefaultDeferredQueryMultiple<EventResponse>(),
            x => x.EventResponseRepository.GetResponseSummaries(eventIds));

        return new GroupEventsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Events = ToGroupPageListEvents(
                upcomingEvents.OrderBy(x => x.Date),
                venues,
                memberResponses,
                responseSummaries,
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

        var (venues, memberResponses, responseSummaries) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByEventIds(eventIds),
            x => currentMember?.IsMemberOf(chapter.Id) == true
                ? x.EventResponseRepository.GetByMemberId(currentMember.Id, eventIds)
                : new DefaultDeferredQueryMultiple<EventResponse>(),
            x => x.EventResponseRepository.GetResponseSummaries(eventIds));

        return new GroupEventsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Events = ToGroupPageListEvents(
                pastEvents.OrderByDescending(x => x.Date),
                venues,
                memberResponses,
                responseSummaries,
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
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
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

        var (venues, memberResponses, responseSummaries) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByEventIds(eventIds),
            x => currentMember?.IsMemberOf(chapter.Id) == true
                ? x.EventResponseRepository.GetByMemberId(currentMember.Id, eventIds)
                : new DefaultDeferredQueryMultiple<EventResponse>(),
            x => x.EventResponseRepository.GetResponseSummaries(eventIds));

        var recentEventViewModels = ToGroupPageListEvents(
            recentEvents.OrderByDescending(x => x.Date),
            venues,
            memberResponses,
            responseSummaries,
            currentMember,
            memberSubscription,
            membershipSettings,
            privacySettings);

        var upcomingEventViewModels = ToGroupPageListEvents(
            upcomingEvents.OrderBy(x => x.Date),
            venues,
            memberResponses,
            responseSummaries,
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
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            Links = links,
            MemberCount = memberCount,
            Owners = adminMembers
                .Select(x => x.Member)
                .Where(x => x.Visible(chapter.Id))
                .ToArray(),
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
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
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
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
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
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
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
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
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
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.SitePaymentSettingsRepository.Get());

        return new GroupProfileSubscriptionPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
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
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.GetByChapterId(chapter.Id));

        return new GroupQuestionsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = questions.Count > 0,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
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

        var (
            currentMember, 
            memberSubscription, 
            membershipSettings, 
            privacySettings, 
            events, 
            links, 
            texts, 
            instagramPosts, 
            latestMembers) = await _unitOfWork.RunAsync(
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

        var (venues, memberResponses, responseSummaries) = await _unitOfWork.RunAsync(
            x => eventIds.Any() 
                ? x.VenueRepository.GetByEventIds(eventIds)
                : new DefaultDeferredQueryMultiple<Venue>(),
            x => eventIds.Any() && currentMemberId != null 
                ? x.EventResponseRepository.GetByMemberId(currentMemberId.Value, eventIds) 
                : new DefaultDeferredQueryMultiple<EventResponse>(),
            x => eventIds.Any()
                ? x.EventResponseRepository.GetResponseSummaries(eventIds)
                : new DefaultDeferredQueryMultiple<EventResponseSummaryDto>());

        var venueDictionary = venues
            .Where(x => _authorizationService.CanViewVenue(x, currentMember, memberSubscription, membershipSettings, privacySettings))
            .ToDictionary(x => x.Id);

        var memberResponseDictionary = memberResponses
            .ToDictionary(x => x.EventId);

        var responseSummaryDictionary = responseSummaries
            .ToDictionary(x => x.EventId);

        var eventResponseViewModels = events
            .Where(x => _authorizationService.CanViewEvent(x, currentMember, memberSubscription, membershipSettings, privacySettings))
            .OrderBy(x => x.Date)
            .Select(x => new EventResponseViewModel(
                @event: x,
                venue: venueDictionary.ContainsKey(x.VenueId) ? venueDictionary[x.VenueId] : null,
                response: memberResponseDictionary.ContainsKey(x.Id) ? memberResponseDictionary[x.Id].Type : EventResponseType.None,
                invited: false,
                responseSummary: responseSummaryDictionary.ContainsKey(x.Id) ? responseSummaryDictionary[x.Id] : null))
            .ToArray();

        return new ChapterHomePageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            Events = eventResponseViewModels,
            InstagramPosts = instagramPosts,
            LatestMembers = latestMembers,
            Links = links,
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
        IEnumerable<EventResponse> memberResponses,
        IEnumerable<EventResponseSummaryDto> responseSummaries,
        Member? currentMember,
        MemberSubscription? memberSubscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        var viewModels = new List<GroupPageListEventViewModel>();

        var venueDictionary = venues.ToDictionary(x => x.Id);
        var memberResponseDictionary = memberResponses.ToDictionary(x => x.EventId);
        var responseSummaryDictionary = responseSummaries.ToDictionary(x => x.EventId);

        foreach (var @event in events)
        {
            var canViewEvent = _authorizationService.CanViewEvent(@event, currentMember, memberSubscription, membershipSettings, privacySettings);
            if (!canViewEvent)
            {
                continue;
            }

            responseSummaryDictionary.TryGetValue(@event.Id, out var responseSummary);
            memberResponseDictionary.TryGetValue(@event.Id, out var memberResponse);

            var venue = venueDictionary[@event.VenueId];
            var canViewVenue = _authorizationService.CanViewVenue(venue, currentMember, memberSubscription, membershipSettings, privacySettings);
            viewModels.Add(new GroupPageListEventViewModel
            {
                Event = @event,
                Response = memberResponse,
                ResponseSummary = responseSummary,
                Venue = canViewVenue ? venue : null
            });
        }     
        
        return viewModels;
    }

    private IDictionary<Guid, double> FilterChaptersByDistance(
        IReadOnlyCollection<Chapter> chapters,
        IReadOnlyCollection<ChapterLocation> chapterLocations,
        LatLong? location,
        Distance distance)
    {
        var chaptersWithDistances = new Dictionary<Guid, double>();

        if (location == null)
        {
            return chaptersWithDistances;
        }        

        var chapterLocationDictionary = chapterLocations
            .ToDictionary(x => x.ChapterId);

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

            var chapterDistance = chapterLocation.LatLong.DistanceFrom(location.Value, distance.Unit);
            if (chapterDistance > distance.Value)
            {
                continue;
            }

            chaptersWithDistances.Add(chapter.Id, chapterDistance);
        }

        return chaptersWithDistances;
    }
}
