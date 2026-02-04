using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Exceptions;
using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Pages;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Events;
using ODK.Data.Core.SocialMedia;
using ODK.Services.Authorization;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Events.ViewModels;
using ODK.Services.SocialMedia;
using ODK.Services.SocialMedia.ViewModels;

namespace ODK.Services.Chapters;

public class ChapterViewModelService : IChapterViewModelService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISocialMediaService _socialMediaService;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterViewModelService(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        ISocialMediaService socialMediaService)
    {
        _authorizationService = authorizationService;
        _socialMediaService = socialMediaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<GroupsViewModel> FindGroups(
        PlatformType platform, Member? currentMember, GroupFilter filter)
    {
        MemberLocation? memberLocation = null;

        if (filter.Location == null && currentMember?.Id != null)
        {
            memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMember.Id);
        }

        var topicGroups = await _unitOfWork.TopicGroupRepository.GetAll().Run();
        var topicGroup = topicGroups
            .FirstOrDefault(x => string.Equals(x.Name, filter.TopicGroup, StringComparison.InvariantCultureIgnoreCase));

        var (chapters, distanceUnits, preferences, adminMembers) = await _unitOfWork.RunAsync(
            x => topicGroup != null
                ? x.ChapterRepository.GetByTopicGroupId(platform, topicGroup.Id)
                : x.ChapterRepository.GetAll(platform),
            x => x.DistanceUnitRepository.GetAll(),
            x => currentMember != null
                ? x.MemberPreferencesRepository.GetByMemberId(currentMember.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberPreferences>(),
            x => currentMember != null
                ? x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id)
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

        if (currentMember != null && distanceUnit.Id != preferences?.DistanceUnitId)
        {
            preferences ??= new MemberPreferences();

            preferences.DistanceUnitId = distanceUnit.Id;

            if (preferences.MemberId == default)
            {
                preferences.MemberId = currentMember.Id;
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
                IsOwner = chapterDictionary[chapterId].OwnerId == currentMember?.Id,
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

    public async Task<AccountMenuChaptersViewModel> GetAccountMenuChaptersViewModel(
        MemberServiceRequest request)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var (adminChapters, memberChapters) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByAdminMemberId(platform, currentMember.Id),
            x => x.ChapterRepository.GetByMemberId(currentMember.Id));

        return new AccountMenuChaptersViewModel
        {
            AdminChapters = adminChapters,
            MemberChapters = memberChapters
        };
    }

    public async Task<ChapterAboutPageViewModel> GetChapterAboutPage(Chapter chapter)
    {
        var chapterId = chapter.Id;

        var (chapterPage, questions, texts) = await _unitOfWork.RunAsync(
            x => x.ChapterPageRepository.GetByChapterId(chapterId, PageType.About),
            x => x.ChapterQuestionRepository.GetByChapterId(chapterId),
            x => x.ChapterTextsRepository.GetByChapterId(chapterId));

        return new ChapterAboutPageViewModel
        {
            ChapterPage = chapterPage,
            Questions = questions
                .OrderBy(x => x.DisplayOrder)
                .ToArray(),
            Texts = texts
        };
    }

    public async Task<ChapterCreateViewModel> GetChapterCreate(
        PlatformType platform, Member currentMember)
    {
        if (platform != PlatformType.Default)
        {
            throw new OdkNotFoundException();
        }

        var (current, memberSubscription, countries, topicGroups, topics, memberTopics) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByOwnerId(currentMember.Id),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMember.Id, platform),
            x => x.CountryRepository.GetAll(),
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll(),
            x => x.MemberTopicRepository.GetByMemberId(currentMember.Id));

        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(currentMember.Id);

        return new ChapterCreateViewModel
        {
            ChapterCount = current.Count,
            ChapterLimit = memberSubscription?.SiteSubscription != null
                ? memberSubscription.SiteSubscription.GroupLimit
                : SiteSubscription.DefaultGroupLimit,
            Countries = countries,
            Member = currentMember,
            MemberLocation = memberLocation,
            MemberTopics = memberTopics,
            Platform = platform,
            TopicGroups = topicGroups,
            Topics = topics
        };
    }

    public async Task<GroupContactPageViewModel> GetGroupContactPage(
        ServiceRequest request, Member? currentMember, Chapter chapter)
    {
        var platform = request.Platform;

        var (
            isAdmin,
            hasProperties,
            hasQuestions,
            conversations,
            membershipSettings,
            privacySettings,
            memberSubscription,
            chapterPages,
            sitePaymentSettings
        ) = await _unitOfWork.RunAsync(
            x => currentMember != null
                ? x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id)
                : new DefaultDeferredQueryAny(false),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => currentMember != null
                ? x.ChapterConversationRepository.GetDtosByMemberId(currentMember.Id, chapter.Id)
                : new DefaultDeferredQueryMultiple<ChapterConversationDto>(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => currentMember != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMember.Id, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id),
            x => x.SitePaymentSettingsRepository.GetActive());

        var canStartConversation = currentMember != null
            ? _authorizationService.CanStartConversation(chapter.Id, currentMember, memberSubscription, membershipSettings, privacySettings)
            : false;

        return new GroupContactPageViewModel
        {
            CanStartConversation = canStartConversation,
            Chapter = chapter,
            ChapterPages = chapterPages,
            Conversations = conversations,
            CurrentMember = currentMember,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            Platform = platform
        };
    }

    public async Task<GroupConversationPageViewModel> GetGroupConversationPage(
        MemberChapterServiceRequest request, Guid conversationId)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (
            isAdmin,
            hasProperties,
            hasQuestions,
            conversations,
            messages,
            notifications,
            chapterPages
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterConversationRepository.GetDtosByMemberId(currentMember.Id, chapter.Id),
            x => x.ChapterConversationMessageRepository.GetDtosByConversationId(conversationId),
            x => x.NotificationRepository.GetUnreadByMemberId(
                currentMember.Id, NotificationType.ConversationAdminMessage, conversationId),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        var dto = conversations
            .FirstOrDefault(x => x.Conversation.Id == conversationId);
        OdkAssertions.Exists(dto);

        var unread = messages
            .Select(x => x.Message)
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
            ChapterPages = chapterPages,
            Conversation = dto.Conversation,
            CurrentMember = currentMember,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            Messages = messages,
            OtherConversations = conversations
                .Where(x => x.Conversation.Id != conversationId)
                .ToArray(),
            Platform = platform
        };
    }

    public async Task<GroupEventsPageViewModel> GetGroupEventsPage(
        ServiceRequest request, Member? currentMember, Chapter chapter)
    {
        var platform = request.Platform;

        var (
            memberSubscription,
            membershipSettings,
            privacySettings,
            isAdmin,
            upcomingEvents,
            hasProperties,
            hasQuestions,
            pastEventCount,
            chapterPages
        ) = await _unitOfWork.RunAsync(
            x => currentMember != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMember.Id, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => currentMember != null
                ? x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id)
                : new DefaultDeferredQueryAny(false),
            x => x.EventRepository.GetByChapterId(chapter.Id, after: DateTime.UtcNow),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.EventRepository.GetPastEventCountByChapterId(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

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
            ChapterPages = chapterPages,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
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

    public async Task<GroupPageViewModel> GetGroupPage(
        ServiceRequest request, Member? currentMember, Chapter chapter)
    {
        var platform = request.Platform;

        var (
            isAdmin,
            hasProperties,
            hasQuestions,
            chapterPages
        ) = await _unitOfWork.RunAsync(
            x => currentMember != null
                ? x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id)
                : new DefaultDeferredQueryAny(false),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        return new GroupPageViewModel
        {
            Chapter = chapter,
            ChapterPages = chapterPages,
            CurrentMember = currentMember,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            Platform = platform
        };
    }

    public async Task<GroupEventsPageViewModel> GetGroupPastEventsPage(
        ServiceRequest request, Member? currentMember, Chapter chapter)
    {
        var platform = request.Platform;

        var (
            memberSubscription,
            membershipSettings,
            privacySettings,
            isAdmin,
            pastEvents,
            hasProperties,
            hasQuestions,
            chapterPages
        ) = await _unitOfWork.RunAsync(
            x => currentMember != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMember.Id, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => currentMember != null
                ? x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id)
                : new DefaultDeferredQueryAny(false),
            x => x.EventRepository.GetRecentEventsByChapterId(chapter.Id, 1000),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

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
            ChapterPages = chapterPages,
            CurrentMember = currentMember,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
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
        ServiceRequest request, Member? currentMember, Chapter chapter)
    {
        var platform = request.Platform;

        var (
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
            hasProperties,
            hasQuestions,
            texts,
            chapterTopics,
            chapterPages
        ) = await _unitOfWork.RunAsync(
            x => currentMember != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMember.Id, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetCountByChapterId(chapter.Id),
            x => x.InstagramPostRepository.GetDtosByChapterId(chapter.Id, 8),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetByChapterId(chapter.Id, after: DateTime.UtcNow),
            x => x.EventRepository.GetRecentEventsByChapterId(chapter.Id, 3),
            x => x.ChapterImageRepository.ExistsForChapterId(chapter.Id),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterTopicRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

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
            ChapterPages = chapterPages,
            CurrentMember = currentMember,
            HasImage = hasImage,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            InstagramPosts = new InstagramPostsViewModel
            {
                Posts = showInstagramFeed
                    ? instagramPosts
                        .Select(x => MapToInstagramPostViewModel(links?.InstagramName ?? string.Empty, x))
                        .ToArray()
                    : []
            },
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMember?.Id),
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            Links = links,
            MemberCount = memberCount,
            Owners = adminMembers
                .Select(x => x.Member)
                .Where(x => x.Visible(chapter.Id))
                .ToArray(),
            Platform = platform,
            RecentEvents = recentEventViewModels,
            Texts = texts,
            Topics = chapterTopics.Select(x => x.Topic).ToArray(),
            UpcomingEvents = upcomingEventViewModels
        };
    }

    public async Task<GroupJoinPageViewModel> GetGroupJoinPage(
        ServiceRequest request, Member? currentMember, Chapter chapter)
    {
        var platform = request.Platform;

        var (
            isAdmin,
            hasQuestions,
            properties,
            propertyOptions,
            texts,
            membershipSettings,
            chapterPages) = await _unitOfWork.RunAsync(
            x => currentMember != null
                ? x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id)
                : new DefaultDeferredQueryAny(false),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        return new GroupJoinPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            ChapterPages = chapterPages,
            HasProfiles = properties.Any(),
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            MembershipSettings = membershipSettings,
            Platform = platform,
            Properties = properties,
            PropertyOptions = propertyOptions,
            RegistrationOpen = chapter.IsOpenForRegistration(),
            Texts = texts
        };
    }

    public async Task<GroupProfilePageViewModel> GetGroupProfilePage(
        MemberChapterServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (
            isAdmin,
            hasQuestions,
            chapterProperties,
            chapterPropertyOptions,
            memberProperties,
            chapterPages
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(currentMember.Id, chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        return new GroupProfilePageViewModel
        {
            Chapter = chapter,
            ChapterPages = chapterPages,
            ChapterProperties = chapterProperties,
            ChapterPropertyOptions = chapterPropertyOptions,
            CurrentMember = currentMember,
            HasProfiles = chapterProperties.Any(),
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = currentMember.IsMemberOf(chapter.Id) == true,
            MemberProperties = memberProperties,
            Platform = platform
        };
    }

    public async Task<GroupQuestionsPageViewModel> GetGroupQuestionsPage(
        ServiceRequest request, Member? currentMember, Chapter chapter)
    {
        var platform = request.Platform;

        var (
            isAdmin,
            hasProperties,
            questions,
            chapterPages) = await _unitOfWork.RunAsync(
            x => currentMember != null
                ? x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id)
                : new DefaultDeferredQueryAny(false),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        return new GroupQuestionsPageViewModel
        {
            Chapter = chapter,
            ChapterPages = chapterPages,
            CurrentMember = currentMember,
            HasProfiles = hasProperties,
            HasQuestions = questions.Count > 0,
            IsAdmin = isAdmin,
            IsMember = currentMember?.IsMemberOf(chapter.Id) == true,
            Platform = platform,
            Questions = questions.OrderBy(x => x.DisplayOrder).ToArray()
        };
    }

    public async Task<GroupSubscriptionPageViewModel> GetGroupSubscriptionPage(
        MemberChapterServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (
            isAdmin,
            hasProperties,
            hasQuestions,
            chapterPages
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.IsAdmin(chapter.Id, currentMember.Id),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        return new GroupSubscriptionPageViewModel
        {
            Chapter = chapter,
            ChapterPages = chapterPages,
            CurrentMember = currentMember,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = currentMember.IsMemberOf(chapter.Id) == true,
            Platform = platform
        };
    }

    public async Task<ChapterHomePageViewModel> GetHomePage(
        ServiceRequest request, Member? currentMember, Chapter chapter)
    {
        var platform = request.Platform;

        var today = chapter.TodayUtc();

        var (
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
            x => currentMember != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMember.Id, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetByChapterId(chapter.Id, today),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
            x => x.InstagramPostRepository.GetDtosByChapterId(chapter.Id, 8),
            x => x.MemberRepository.GetLatestByChapterId(chapter.Id, 8),
            x => x.ChapterTopicRepository.GetByChapterId(chapter.Id));

        var chapterLocation = await _unitOfWork.ChapterLocationRepository.GetByChapterId(chapter.Id);

        var eventIds = events.Select(x => x.Id).ToArray();

        var (venues, memberResponses, responseSummaries) = await _unitOfWork.RunAsync(
            x => eventIds.Any()
                ? x.VenueRepository.GetByEventIds(eventIds)
                : new DefaultDeferredQueryMultiple<Venue>(),
            x => eventIds.Any() && currentMember != null
                ? x.EventResponseRepository.GetByMemberId(currentMember.Id, eventIds)
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
            .Where(x => _authorizationService.CanViewEvent(
                x, currentMember, memberSubscription, membershipSettings, privacySettings))
            .OrderBy(x => x.Date)
            .Select(x => new EventResponseViewModel(
                @event: x,
                venue: venueDictionary.ContainsKey(x.VenueId) ? venueDictionary[x.VenueId] : null,
                response: memberResponseDictionary.ContainsKey(x.Id)
                    ? memberResponseDictionary[x.Id].Type
                    : EventResponseType.None,
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
            InstagramPosts = new InstagramPostsViewModel
            {
                Posts = showInstagramFeed
                    ? instagramPosts
                        .Select(x => MapToInstagramPostViewModel(links?.InstagramName ?? string.Empty, x))
                        .ToArray()
                    : []
            },
            LatestMembers = latestMembers,
            Links = links,
            Platform = platform,
            Texts = texts,
            Topics = chapterTopics.Select(x => x.Topic).ToArray(),
            WhatsAppUrl =
                !string.IsNullOrEmpty(links?.WhatsApp) &&
                memberSubscription?.Type == SubscriptionType.Full &&
                !memberSubscription.IsExpired()
                    ? _socialMediaService.GetWhatsAppLink(links.WhatsApp)
                    : null
        };
    }

    public async Task<MemberChaptersViewModel> GetMemberChapters(MemberServiceRequest request)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var (chapters, adminMembers) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByMemberId(currentMember.Id),
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id));

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
                IsOwner = chapter.OwnerId == currentMember.Id,
                // no need to show location for existing groups
                Location = null,
                Platform = platform,
                Texts = chapterTexts,
                // no need to show topics for existing groups
                Topics = []
            };

            if (chapter.OwnerId == currentMember.Id)
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

    private InstagramPostViewModel MapToInstagramPostViewModel(string username, InstagramPostDto dto)
        => new InstagramPostViewModel
        {
            Caption = dto.Post.Caption,
            ExternalId = dto.Post.ExternalId,
            Images = dto.Images
                .Select(x => new InstagramImageMetadataViewModel
                {
                    Alt = x.Alt,
                    Id = x.Id
                })
                .ToArray(),
            Url = _socialMediaService.GetInstagramPostUrl(dto.Post.ExternalId),
            Username = username
        };
}