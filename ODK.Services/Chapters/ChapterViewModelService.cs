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
using ODK.Core.Subscriptions;
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
    private readonly IUnitOfWork _unitOfWork;

    public ChapterViewModelService(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<GroupsViewModel> FindGroups(
        PlatformType platform, Guid? currentMemberId, GroupFilter filter)
    {
        MemberLocation? memberLocation = null;

        if (filter.Location == null && currentMemberId != null)
        {
            memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMemberId.Value);
        }

        var topicGroups = await _unitOfWork.TopicGroupRepository.GetAll().Run();
        var topicGroup = topicGroups
            .FirstOrDefault(x => string.Equals(x.Name, filter.TopicGroup, StringComparison.InvariantCultureIgnoreCase));

        var (chapters, distanceUnits, currentMember, preferences, adminMembers) = await _unitOfWork.RunAsync(
            x => topicGroup != null 
                ? x.ChapterRepository.GetByTopicGroupId(topicGroup.Id)
                : x.ChapterRepository.GetAll(),
            x => x.DistanceUnitRepository.GetAll(),
            x => currentMemberId != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.MemberPreferencesRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<MemberPreferences>(),
            x => currentMemberId != null
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId.Value)
                : new DefaultDeferredQueryMultiple<ChapterAdminMember>());

        // TODO: search by location in the database
        var chapterLocations = await _unitOfWork.ChapterLocationRepository.GetByChapterIds(chapters.Select(x => x.Id));
        
        distanceUnits = distanceUnits
            .OrderBy(x => x.Order)
            .ToArray();

        var distanceUnit = distanceUnits
            .FirstOrDefault(x => string.Equals(x.Abbreviation, filter.DistanceUnit, StringComparison.OrdinalIgnoreCase))
            ?? distanceUnits.FirstOrDefault(x => x.Id == preferences?.DistanceUnitId)
            ?? distanceUnits.First();

        if (currentMemberId != null && distanceUnit.Id != preferences?.DistanceUnitId)
        {
            preferences ??= new MemberPreferences();

            preferences.DistanceUnitId = distanceUnit.Id;
            
            if (preferences.MemberId == default)
            {
                preferences.MemberId = currentMemberId.Value;
                _unitOfWork.MemberPreferencesRepository.Add(preferences);                
            }
            else
            {
                _unitOfWork.MemberPreferencesRepository.Update(preferences);
            }

            await _unitOfWork.SaveChangesAsync();
        }

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

        var chapterIds = chapters
            .Select(x => x.Id)
            .ToArray();

        var (texts, chapterTopics, chapterImageDtos) = await _unitOfWork.RunAsync(
            x => chapterIds.Length > 0 
                ? x.ChapterTextsRepository.GetByChapterIds(chapterIds)
                : new DefaultDeferredQueryMultiple<ChapterTexts>(),
            x => chapterIds.Length > 0
                ? x.ChapterTopicRepository.GetDtosByChapterIds(chapterIds)
                : new DefaultDeferredQueryMultiple<ChapterTopicDto>(),
            x => chapterIds.Length > 0 
                ? x.ChapterImageRepository.GetDtosByChapterIds(chapterIds)
                : new DefaultDeferredQueryMultiple<ChapterImageMetadata>());

        var chapterDictionary = chapters
            .ToDictionary(x => x.Id);
        var chapterLocationDictionary = chapterLocations
            .ToDictionary(x => x.ChapterId);

        var chapterTopicsDictionary = chapterTopics
            .GroupBy(x => x.ChapterId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var imageDictionary = chapterImageDtos
            .ToDictionary(x => x.ChapterId);

        var textsDictionary = texts
            .ToDictionary(x => x.ChapterId);

        var groups = new List<ChapterWithDistanceViewModel>();
        foreach (var chapterId in chaptersWithDistances.Keys)
        {
            chapterLocationDictionary.TryGetValue(chapterId, out var chapterLocation);
            textsDictionary.TryGetValue(chapterId, out var chapterTexts);
            chapterTopicsDictionary.TryGetValue(chapterId, out var topics);

            var hasImage = imageDictionary.ContainsKey(chapterId);

            groups.Add(new ChapterWithDistanceViewModel
            {
                Chapter = chapterDictionary[chapterId],
                Distance = chaptersWithDistances[chapterId],
                HasImage = hasImage,
                IsAdmin = adminMembers.Any(x => x.ChapterId == chapterId),
                IsMember = currentMember?.IsMemberOf(chapterId) == true,
                Location = chapterLocation,
                Platform = platform,
                Texts = chapterTexts,
                Topics = topics?.Select(x => x.Topic).ToArray() ?? []
            });
        }

        return new GroupsViewModel
        {
            Distance = new Distance { Unit = distanceUnit, Value = distance },
            DistanceUnits = distanceUnits,
            Groups = groups
                .OrderBy(x => x.Distance!.Value)
                .ToArray(),
            Location = location,
            Platform = platform,
            TopicGroupId = topicGroup?.Id,
            TopicGroups = topicGroups
        };
    }

    public async Task<ChapterCreateViewModel> GetChapterCreate(
        PlatformType platform, Guid currentMemberId)
    {
        if (platform != PlatformType.Default)
        {
            throw new OdkNotFoundException();
        }

        var (current, member, memberSubscription, countries, topicGroups, topics, memberTopics) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByOwnerId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMemberId, platform),
            x => x.CountryRepository.GetAll(),
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll(),
            x => x.MemberTopicRepository.GetByMemberId(currentMemberId));

        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMemberId);

        return new ChapterCreateViewModel
        {
            ChapterCount = current.Count,
            ChapterLimit = memberSubscription?.SiteSubscription != null
                ? memberSubscription.SiteSubscription.GroupLimit
                : SiteSubscription.DefaultGroupLimit,
            Countries = countries,
            Member = member,
            MemberLocation = memberLocation,
            MemberTopics = memberTopics,
            Platform = platform,
            TopicGroups = topicGroups,
            Topics = topics
        };
    }

    public async Task<GroupContactPageViewModel> GetGroupContactPage(
        ServiceRequest request, Guid? currentMemberId, string slug)
    {
        var platform = request.Platform;

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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

    public async Task<GroupConversationPageViewModel> GetGroupConversationPage(
        MemberServiceRequest request, string slug, Guid conversationId)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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

    public async Task<GroupEventsPageViewModel> GetGroupEventsPage(
        ServiceRequest request, Guid? currentMemberId, string slug)
    {
        var platform = request.Platform;

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

        var (
            currentMember,
            memberSubscription,
            ownerSubscription,
            membershipSettings,
            privacySettings,
            adminMembers,
            upcomingEvents,
            hasQuestions,
            pastEventCount
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
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.EventRepository.GetPastEventCountByChapterId(chapter.Id));

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
            PastEventCount = pastEventCount,
            Platform = platform          
        };
    }

    public async Task<GroupPageViewModel> GetGroupPage(ServiceRequest request, Guid? currentMemberId, string slug)
    {
        var platform = request.Platform;

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id));

        return new GroupPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<GroupEventsPageViewModel> GetGroupPastEventsPage(
        ServiceRequest request, Guid? currentMemberId, string slug)
    {
        var platform = request.Platform;

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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
            PastEventCount = pastEvents.Count,
            Platform = platform
        };
    }

    public async Task<GroupHomePageViewModel> GetGroupHomePage(
        ServiceRequest request, Guid? currentMemberId, string slug)
    {
        var platform = request.Platform;

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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
            hasImage,
            hasQuestions, 
            texts,
            chapterTopics
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
            x => x.ChapterImageRepository.ExistsForChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterTopicRepository.GetByChapterId(chapter.Id));

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

        var showInstagramFeed = ownerSubscription?.HasFeature(SiteFeatureType.InstagramFeed) == true &&
            privacySettings?.InstagramFeed != false;

        return new GroupHomePageViewModel
        {
            Chapter = chapter,
            ChapterLocation = location,
            CurrentMember = currentMember,
            HasImage = hasImage,
            HasQuestions = hasQuestions,
            InstagramPosts = showInstagramFeed ? instagramPosts : [],
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
            Topics = chapterTopics.Select(x => x.Topic).ToArray(),
            UpcomingEvents = upcomingEventViewModels
        };
    }
    
    public async Task<GroupJoinPageViewModel> GetGroupJoinPage(ServiceRequest request, Guid? currentMemberId, string slug)
    {
        var platform = request.Platform;

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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

    public async Task<GroupProfilePageViewModel> GetGroupProfilePage(MemberServiceRequest request, string slug)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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

    public async Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(
        ServiceRequest request, Guid? currentMemberId, string slug)
    {
        var platform = request.Platform;

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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

    public async Task<GroupSubscriptionPageViewModel> GetGroupSubscriptionPage(
        MemberServiceRequest request, string slug)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

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
            x => x.SitePaymentSettingsRepository.GetActive());

        return new GroupSubscriptionPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember.IsMemberOf(chapter.Id) == true,
            OwnerSubscription = ownerSubscription,
            Currency = chapterPaymentSettings.Currency,
            Platform = platform,
            SitePaymentSettings = sitePaymentSettings
        };
    }

    public async Task<ChapterHomePageViewModel> GetHomePage(ServiceRequest request, Guid? currentMemberId, string chapterName)
    {        
        var platform = request.Platform;

        var chapter = await GetChapter(chapterName);
        
        var today = chapter.TodayUtc();

        var (
            currentMember, 
            memberSubscription, 
            ownerSubscription,
            membershipSettings, 
            privacySettings, 
            events, 
            links, 
            texts, 
            instagramPosts, 
            latestMembers,
            chapterTopics) = await _unitOfWork.RunAsync(
            x => currentMemberId != null 
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) 
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetByChapterId(chapter.Id, today),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
            x => x.InstagramPostRepository.GetByChapterId(chapter.Id, 8),
            x => x.MemberRepository.GetLatestByChapterId(chapter.Id, 8),
            x => x.ChapterTopicRepository.GetByChapterId(chapter.Id));

        var chapterLocation = await _unitOfWork.ChapterLocationRepository.GetByChapterId(chapter.Id);

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

        var showInstagramFeed = ownerSubscription?.HasFeature(SiteFeatureType.InstagramFeed) == true &&
            privacySettings?.InstagramFeed != false;

        return new ChapterHomePageViewModel
        {
            Chapter = chapter,
            ChapterLocation = chapterLocation,
            CurrentMember = currentMember,
            Events = eventResponseViewModels,
            InstagramPosts = showInstagramFeed ? instagramPosts : [],
            LatestMembers = latestMembers,
            Links = links,
            Platform = platform,
            Texts = texts,
            Topics = chapterTopics.Select(x => x.Topic).ToArray()
        };
    }

    public async Task<MemberChaptersViewModel> GetMemberChapters(MemberServiceRequest request)
    {
        var (memberId, platform) = (request.CurrentMemberId, request.Platform);

        var (chapters, adminMembers) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByMemberId(memberId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(memberId));

        var chapterIds = chapters
            .Select(x => x.Id)
            .ToArray();

        var (texts, images) = await _unitOfWork.RunAsync(
            x => chapterIds.Length > 0 
                ? x.ChapterTextsRepository.GetByChapterIds(chapterIds)
                : new DefaultDeferredQueryMultiple<ChapterTexts>(),
            x => chapterIds.Length > 0
                ? x.ChapterImageRepository.GetDtosByChapterIds(chapterIds)
                : new DefaultDeferredQueryMultiple<ChapterImageMetadata>());

        var adminMemberDictionary = adminMembers.ToDictionary(x => x.ChapterId);
        var imageDictionary = images.ToDictionary(x => x.ChapterId);
        var textsDictionary = texts.ToDictionary(x => x.ChapterId);
        
        var admin = new List<ChapterWithDistanceViewModel>();
        var member = new List<ChapterWithDistanceViewModel>();
        var owned = new List<ChapterWithDistanceViewModel>();

        foreach (var chapter in chapters)
        {
            adminMemberDictionary.TryGetValue(chapter.Id, out var adminMember);
            imageDictionary.TryGetValue(chapter.Id, out var image);
            textsDictionary.TryGetValue(chapter.Id, out var chapterTexts);
            
            var viewModel = new ChapterWithDistanceViewModel
            {
                Chapter = chapter,
                Distance = null,
                HasImage = image != null,
                IsAdmin = adminMember != null,
                IsMember = true,
                // no need to show location for existing groups
                Location = null,
                Platform = platform,
                Texts = chapterTexts,
                // no need to show topics for existing groups
                Topics = []
            };

            if (chapter.OwnerId == memberId)
            {
                owned.Add(viewModel);
            }
            else if (viewModel.IsAdmin)
            {
                admin.Add(viewModel);
            }
            else
            {
                member.Add(viewModel);
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
        return OdkAssertions.Exists(chapter, $"Chapter not found: '{name}'");
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

    private IDictionary<Guid, Distance> FilterChaptersByDistance(
        IReadOnlyCollection<Chapter> chapters,
        IReadOnlyCollection<ChapterLocation> chapterLocations,
        LatLong? location,
        Distance distance)
    {
        var chaptersWithDistances = new Dictionary<Guid, Distance>();

        if (location == null)
        {
            return chapters
                .ToDictionary(x => x.Id, x => new Distance { Unit = distance.Unit, Value = 0 });
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

            chaptersWithDistances.Add(
                chapter.Id, 
                new Distance { Unit = distance.Unit, Value = chapterDistance });
        }

        return chaptersWithDistances;
    }
}
