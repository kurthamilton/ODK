using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Cryptography;
using ODK.Core.DataTypes;
using ODK.Core.Emails;
using ODK.Core.Exceptions;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Geolocation;
using ODK.Services.Logging;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Topics;
using ODK.Services.Topics.Models;

namespace ODK.Services.Members;

public class MemberService : IMemberService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IGeolocationService _geolocationService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberImageService _memberImageService;
    private readonly INotificationService _notificationService;
    private readonly IOAuthProviderFactory _oauthProviderFactory;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ITopicService _topicService;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        ICacheService cacheService,
        IMemberImageService memberImageService,
        IMemberEmailService memberEmailService,
        INotificationService notificationService,
        IOAuthProviderFactory oauthProviderFactory,
        ITopicService topicService,
        IPaymentProviderFactory paymentProviderFactory,
        IGeolocationService geolocationService,
        ILoggingService loggingService)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _geolocationService = geolocationService;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _memberImageService = memberImageService;
        _notificationService = notificationService;
        _oauthProviderFactory = oauthProviderFactory;
        _paymentProviderFactory = paymentProviderFactory;
        _topicService = topicService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CancelChapterSubscription(Guid memberId, string externalId)
    {
        var (member, memberSubscriptionRecord) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRecordRepository.GetByExternalId(externalId));

        if (memberSubscriptionRecord.MemberId != member.Id)
        {
            throw new OdkNotFoundException();
        }

        return await CancelSubscription(memberSubscriptionRecord);
    }

    public async Task<ServiceResult> ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken)
    {
        var (member, token) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberEmailAddressUpdateTokenRepository.GetByMemberId(memberId));
        if (token == null)
        {
            return ServiceResult.Failure("Invalid link");
        }

        if (token.ConfirmationToken != confirmationToken)
        {
            return ServiceResult.Failure("Invalid link");
        }

        _unitOfWork.MemberEmailAddressUpdateTokenRepository.Delete(token);

        var existing = await _unitOfWork.MemberRepository.GetByEmailAddress(token.NewEmailAddress).Run();
        if (existing != null)
        {
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.Failure("Email not updated: new email address is already in use");
        }

        member.EmailAddress = token.NewEmailAddress;
        _unitOfWork.MemberRepository.Update(member);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult<Member?>> CreateAccount(IServiceRequest request, AccountCreateModel model)
    {
        var (existing, siteSubscription, distanceUnits, topics) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByEmailAddress(model.EmailAddress),
            x => x.SiteSubscriptionRepository.GetDefault(request.Platform),
            x => x.DistanceUnitRepository.GetAll(),
            x => x.TopicRepository.GetByIds(model.TopicIds));

        if (existing != null)
        {
            await _memberEmailService.SendDuplicateMemberEmail(request, null, existing);
            return ServiceResult<Member?>.Successful(null);
        }

        var timeZone = model.Location != null
            ? await _geolocationService.GetTimeZoneFromLocation(model.Location.Value)
            : null;

        var member = new Member
        {
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = model.EmailAddress,
            FirstName = model.FirstName,
            LastName = model.LastName,
            TimeZone = timeZone
        };

        if (model.OAuthProviderType != null && !string.IsNullOrEmpty(model.OAuthToken))
        {
            var oauthProvider = _oauthProviderFactory.GetProvider(model.OAuthProviderType.Value);
            var oauthUser = await oauthProvider.GetUser(model.OAuthToken);
            if (string.Equals(oauthUser.Email, model.EmailAddress, StringComparison.InvariantCultureIgnoreCase))
            {
                member.Activated = true;
            }
        }

        _unitOfWork.MemberRepository.Add(member);

        if (model.Location != null)
        {
            var country = await _geolocationService.GetCountryFromLocation(model.Location.Value);

            _unitOfWork.MemberLocationRepository.Add(new MemberLocation
            {
                CountryId = country?.Id,
                MemberId = member.Id,
                LatLong = model.Location.Value,
                Name = model.LocationName
            });

            if (country?.CurrencyId != null)
            {
                _unitOfWork.MemberPaymentSettingsRepository.Add(new MemberPaymentSettings
                {
                    CurrencyId = country.CurrencyId,
                    MemberId = member.Id
                });
            }
        }

        var distanceUnit = distanceUnits
            .OrderBy(x => x.Order)
            .First();
        _unitOfWork.MemberPreferencesRepository.Add(new MemberPreferences
        {
            DistanceUnitId = distanceUnit.Id,
            MemberId = member.Id
        });

        _unitOfWork.MemberSiteSubscriptionRepository.Add(new MemberSiteSubscription
        {
            MemberId = member.Id,
            SiteSubscriptionId = siteSubscription.Id
        });

        string? activationToken = null;
        if (!member.Activated)
        {
            activationToken = TokenGenerator.GenerateBase64Token(64);
            _unitOfWork.MemberActivationTokenRepository.Add(new MemberActivationToken
            {
                ActivationToken = activationToken,
                MemberId = member.Id
            });
        }

        if (topics.Count > 0)
        {
            _unitOfWork.MemberTopicRepository.AddMany(topics.Select(x => new MemberTopic
            {
                MemberId = member.Id,
                TopicId = x.Id
            }));
        }

        await _unitOfWork.SaveChangesAsync();

        await _topicService.AddNewMemberTopics(
            MemberServiceRequest.Create(member, request),
            model.NewTopics);

        if (!string.IsNullOrEmpty(activationToken))
        {
            await _memberEmailService.SendActivationEmail(request, null, member, activationToken);
        }
        else
        {
            await _memberEmailService.SendSiteWelcomeEmail(request, member);
        }

        return ServiceResult<Member?>.Successful(member);
    }

    public async Task<ServiceResult> CreateChapterAccount(IChapterServiceRequest request, MemberCreateProfile model)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        await _loggingService.Info($"Creating chapter account for {model.EmailAddress}");

        var (
            chapterProperties,
            membershipSettings,
            existing,
            siteSubscription,
            ownerSubscription
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetByEmailAddress(model.EmailAddress),
            x => x.SiteSubscriptionRepository.GetDefault(platform),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id));

        var validationResult = ValidateMemberProfile(chapterProperties, model, forApplication: true);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        var image = new MemberImage();
        var avatar = new MemberAvatar();

        var imageValidationResult = _memberImageService.UpdateMemberImage(image, avatar, model.ImageData);
        if (!imageValidationResult.Success)
        {
            return imageValidationResult;
        }

        if (existing != null)
        {
            await _memberEmailService.SendDuplicateMemberEmail(request, chapter, existing);
            return ServiceResult.Successful();
        }

        var now = DateTime.UtcNow;

        var member = new Member
        {
            Activated = false,
            CreatedUtc = now,
            EmailAddress = model.EmailAddress,
            FirstName = model.FirstName,
            LastName = model.LastName,
            SiteAdmin = false,
            TimeZone = chapter.TimeZone
        };

        _unitOfWork.MemberRepository.Add(member);

        if (model.EmailOptIn != true)
        {
            _unitOfWork.MemberEmailPreferenceRepository.Add(new MemberEmailPreference
            {
                Disabled = true,
                MemberId = member.Id,
                Type = MemberEmailPreferenceType.Events
            });
        }

        var memberProperties = model
            .Properties
            .Select(x => x.ToMemberProperty(member.Id))
            .ToArray();

        AddMemberToChapter(now, member, chapter, memberProperties, membershipSettings, ownerSubscription);

        var chapterLocation = await _unitOfWork.ChapterLocationRepository.GetByChapterId(chapter.Id);
        if (chapterLocation != null)
        {
            _unitOfWork.MemberLocationRepository.Add(new MemberLocation
            {
                CountryId = chapter.CountryId,
                MemberId = member.Id,
                LatLong = chapterLocation.LatLong,
                Name = chapterLocation.Name
            });
        }

        _unitOfWork.MemberSiteSubscriptionRepository.Add(new MemberSiteSubscription
        {
            MemberId = member.Id,
            SiteSubscriptionId = siteSubscription.Id
        });

        image.MemberId = member.Id;
        _unitOfWork.MemberImageRepository.Add(image);

        avatar.MemberId = member.Id;
        _unitOfWork.MemberAvatarRepository.Add(avatar);

        var activationToken = TokenGenerator.GenerateBase64Token(64);
        _unitOfWork.MemberActivationTokenRepository.Add(new MemberActivationToken
        {
            ActivationToken = activationToken,
            ChapterId = chapter.Id,
            MemberId = member.Id
        });

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // double check the existence of the user in the DB to see if this was a double submission
            existing = await _unitOfWork.MemberRepository.GetByEmailAddress(model.EmailAddress).Run();

            if (existing != null)
            {
                await _loggingService.Info(
                    $"Chapter account create: double submission detected for '{model.EmailAddress}', returning OK");
                return ServiceResult.Successful();
            }

            await _loggingService.Error($"Error creating chapter account for '{model.EmailAddress}'", ex);
            return ServiceResult.Failure("An error occurred when creating your account. Please try again.");
        }

        try
        {
            await _memberEmailService.SendActivationEmail(request, chapter, member, activationToken);

            return ServiceResult.Successful();
        }
        catch
        {
            return ServiceResult.Failure("Your account has been created but an error occurred sending an email.");
        }
    }

    public async Task<ServiceResult> DeleteMember(IMemberServiceRequest request)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var chapters = await _unitOfWork.ChapterRepository
            .GetByOwnerId(platform, currentMember.Id)
            .Run();

        var activeChapters = chapters
            .Where(x => x.IsOpenForRegistration())
            .ToArray();
        if (activeChapters.Length > 0)
        {
            return ServiceResult.Failure("Group owners cannot delete their account");
        }

        foreach (var chapter in chapters)
        {
            chapter.OwnerId = null;
            _unitOfWork.ChapterRepository.Update(chapter);
        }

        _unitOfWork.MemberRepository.Delete(currentMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteMemberChapterData(IMemberChapterServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (chapterAdminMembers, memberProperties, notifications) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(currentMember.Id, chapter.Id),
            x => x.NotificationRepository.GetByMemberId(currentMember.Id, chapter.Id));

        if (chapter.OwnerId == currentMember.Id)
        {
            return ServiceResult.Failure("Group owners cannot leave their own groups");
        }

        var memberChapter = currentMember.MemberChapter(chapter.Id);
        if (memberChapter == null)
        {
            return ServiceResult.Failure("Member is not a member of this group");
        }

        currentMember.Chapters.Remove(memberChapter);
        _unitOfWork.MemberChapterRepository.Delete(memberChapter);

        var chapterAdminMember = chapterAdminMembers
            .FirstOrDefault(x => x.MemberId == currentMember.Id);
        if (chapterAdminMember != null)
        {
            _unitOfWork.ChapterAdminMemberRepository.Delete(chapterAdminMember);
        }

        _unitOfWork.MemberPropertyRepository.DeleteMany(memberProperties);
        _unitOfWork.NotificationRepository.DeleteMany(notifications);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<Member?> FindMemberByEmailAddress(string emailAddress)
    {
        return await _unitOfWork.MemberRepository.GetByEmailAddress(emailAddress).Run();
    }

    public async Task<VersionedServiceResult<MemberAvatar>> GetMemberAvatar(long? currentVersion, Guid memberId)
    {
        var result = await _cacheService.GetOrSetVersionedItem(
            () => _unitOfWork.MemberAvatarRepository.GetByMemberId(memberId).Run(),
            memberId,
            currentVersion);

        if (currentVersion == result.Version)
        {
            return result;
        }

        var image = result.Value;
        if (image == null)
        {
            return new VersionedServiceResult<MemberAvatar>(0, null);
        }

        var version = BitConverter.ToInt64(image.Version);
        return new VersionedServiceResult<MemberAvatar>(version, image);
    }

    public async Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId)
    {
        var result = await _cacheService.GetOrSetVersionedItem(
            () => _unitOfWork.MemberImageRepository.GetByMemberId(memberId).Run(),
            memberId,
            currentVersion);

        if (currentVersion == result.Version)
        {
            return result;
        }

        var image = result.Value;
        return image != null
            ? new VersionedServiceResult<MemberImage>(BitConverter.ToInt64(image.Version), image)
            : new VersionedServiceResult<MemberImage>(0, null);
    }

    public async Task<MemberLocationViewModel> GetMemberLocationViewModel(IMemberServiceRequest request)
    {
        var (distanceUnits, memberPreferences) = await _unitOfWork.RunAsync(
            x => x.DistanceUnitRepository.GetAll(),
            x => x.MemberPreferencesRepository.GetByMemberId(request.CurrentMember.Id));

        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(request.CurrentMember.Id);

        return new MemberLocationViewModel
        {
            DistanceUnits = distanceUnits,
            MemberLocation = memberLocation,
            MemberPreferences = memberPreferences
        };
    }

    public async Task<MemberSubscriptionAlertViewModel> GetMemberSubscriptionAlertViewModel(
        Guid memberId, Guid chapterId)
    {
        var (memberSubscription, chapterMembershipSettings) = await _unitOfWork.RunAsync(
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        return new MemberSubscriptionAlertViewModel
        {
            ChapterMembershipSettings = chapterMembershipSettings,
            MemberSubscription = memberSubscription
        };
    }

    public async Task<ServiceResult> JoinChapter(
        IMemberChapterServiceRequest request, IEnumerable<MemberPropertyUpdateModel> properties)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (adminMembers,
            notificationSettings,
            ownerSubscription,
            members,
            chapterProperties,
            chapterPropertyOptions,
            membershipSettings
            ) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.NewMember),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetCountByChapterId(chapter.Id),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

        if (currentMember.IsMemberOf(chapter.Id))
        {
            return ServiceResult.Failure("You are already a member of this group");
        }

        var registrationResult = ChapterIsOpenForRegistration(platform, members, ownerSubscription);
        if (!registrationResult.Success)
        {
            return registrationResult;
        }

        var validationResult = ValidateMemberProperties(chapterProperties, properties, forApplication: true);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        var memberProperties = properties
            .Select(x => x.ToMemberProperty(currentMember.Id))
            .ToArray();

        AddMemberToChapter(DateTime.UtcNow, currentMember, chapter, memberProperties, membershipSettings, ownerSubscription);

        _notificationService.AddNewMemberNotifications(currentMember, chapter.Id, adminMembers, notificationSettings);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendNewMemberAdminEmail(
            request,
            adminMembers,
            currentMember,
            chapterProperties,
            memberProperties);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> LeaveChapter(IMemberChapterServiceRequest request, string reason)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (adminMembers, subscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberSubscriptionRecordRepository.GetLatest(currentMember.Id, chapter.Id));

        var result = await DeleteMemberChapterData(request);
        if (!result.Success)
        {
            return result;
        }

        if (subscription != null)
        {
            await CancelSubscription(subscription);
        }

        await _memberEmailService.SendMemberLeftChapterEmail(
            request,
            adminMembers,
            currentMember,
            reason);

        return ServiceResult.Successful($"You have left the group '{chapter.GetDisplayName(request.Platform)}'");
    }

    public async Task<ServiceResult> RequestMemberEmailAddressUpdate(IMemberChapterServiceRequest request, string newEmailAddress)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var existingToken = await _unitOfWork.MemberEmailAddressUpdateTokenRepository
            .GetByMemberId(currentMember.Id)
            .Run();

        return await RequestMemberEmailAddressUpdate(
            request,
            chapter,
            currentMember,
            newEmailAddress,
            existingToken);
    }

    public async Task<ServiceResult> RequestMemberEmailAddressUpdate(IMemberServiceRequest request, string newEmailAddress)
    {
        var currentMember = request.CurrentMember;

        var existingToken = await _unitOfWork.MemberEmailAddressUpdateTokenRepository
            .GetByMemberId(currentMember.Id).Run();

        return await RequestMemberEmailAddressUpdate(
            request,
            null,
            currentMember,
            newEmailAddress,
            existingToken);
    }

    public async Task RotateMemberImage(Guid memberId)
    {
        var (member, image, avatar) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberImageRepository.GetByMemberId(memberId),
            x => x.MemberAvatarRepository.GetByMemberId(memberId));

        if (image == null)
        {
            return;
        }

        avatar ??= new MemberAvatar();

        _memberImageService.RotateMemberImage(image, avatar);

        _unitOfWork.MemberImageRepository.Update(image);

        if (avatar.MemberId == Guid.Empty)
        {
            avatar.MemberId = memberId;
            _unitOfWork.MemberAvatarRepository.Add(avatar);
        }
        else
        {
            _unitOfWork.MemberAvatarRepository.Update(avatar);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<MemberImage>(memberId);
        _cacheService.RemoveVersionedItem<MemberAvatar>(memberId);
    }

    public async Task<ChapterSubscriptionCheckoutStartedViewModel> StartChapterSubscriptionCheckoutSession(
        IMemberChapterServiceRequest request, Guid chapterSubscriptionId, string returnPath)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (sitePaymentSettings,
            chapterPaymentAccount,
            chapterSubscription) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapter.Id),
            x => x.ChapterSubscriptionRepository.GetById(chapterSubscriptionId));

        OdkAssertions.BelongsToChapter(chapterSubscription, chapter.Id);

        if (string.IsNullOrEmpty(chapterSubscription.ExternalId))
        {
            throw new Exception("Error starting checkout session: chapterSubscription.ExternalId missing");
        }

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            sitePaymentSettings,
            chapterPaymentAccount);

        var subscriptionPlan = await paymentProvider.GetSubscriptionPlan(chapterSubscription.ExternalId);
        if (subscriptionPlan == null)
        {
            throw new Exception("Error starting checkout session: subscriptionPlan not found");
        }

        var utcNow = DateTime.UtcNow;
        var paymentCheckoutSessionId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var metadata = new PaymentMetadataModel(
            PaymentReasonType.ChapterSubscription,
            currentMember,
            chapterSubscription,
            paymentCheckoutSessionId: paymentCheckoutSessionId,
            paymentId: paymentId);

        var externalCheckoutSession = await paymentProvider.StartCheckout(
            request,
            currentMember.EmailAddress,
            subscriptionPlan,
            returnPath,
            metadata);

        _unitOfWork.PaymentCheckoutSessionRepository.Add(new PaymentCheckoutSession
        {
            Id = paymentCheckoutSessionId,
            MemberId = currentMember.Id,
            PaymentId = paymentId,
            SessionId = externalCheckoutSession.SessionId,
            StartedUtc = utcNow
        });

        _unitOfWork.PaymentRepository.Add(new Payment
        {
            Amount = (decimal)chapterSubscription.Amount,
            ChapterId = chapterSubscription.ChapterId,
            CreatedUtc = utcNow,
            CurrencyId = chapterSubscription.Currency.Id,
            ExternalId = externalCheckoutSession.PaymentId,
            Id = paymentId,
            MemberId = currentMember.Id,
            Reference = chapterSubscription.ToReference(),
            SitePaymentSettingId = chapterSubscription.SitePaymentSettingId
        });

        await _unitOfWork.SaveChangesAsync();

        return new ChapterSubscriptionCheckoutStartedViewModel
        {
            Chapter = chapter,
            ChapterSubscription = chapterSubscription,
            ClientSecret = externalCheckoutSession.ClientSecret,
            PaymentSettings = sitePaymentSettings,
            Platform = platform
        };
    }

    public async Task<ServiceResult> UpdateMemberEmailPreferences(Guid id, IEnumerable<MemberEmailPreferenceType> disabledTypes)
    {
        var preferences = await _unitOfWork.MemberEmailPreferenceRepository
            .GetByMemberId(id)
            .Run();

        var preferenceDictionary = preferences
            .ToDictionary(x => x.Type);

        foreach (var type in disabledTypes)
        {
            preferenceDictionary.TryGetValue(type, out var preference);

            if (preference == null)
            {
                _unitOfWork.MemberEmailPreferenceRepository.Add(new MemberEmailPreference
                {
                    Disabled = true,
                    MemberId = id,
                    Type = type
                });
            }
            else if (!preference.Disabled)
            {
                preference.Disabled = true;
                _unitOfWork.MemberEmailPreferenceRepository.Update(preference);
            }
        }

        foreach (var type in preferenceDictionary.Keys)
        {
            if (disabledTypes.Contains(type))
            {
                continue;
            }

            var preference = preferenceDictionary[type];
            _unitOfWork.MemberEmailPreferenceRepository.Delete(preference);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberChapterProfile(
        IMemberChapterServiceRequest request, MemberChapterProfileUpdateModel model)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterProperties, memberProperties) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(currentMember.Id, chapter.Id));

        var validationResult = ValidateMemberProfile(chapterProperties, model, forApplication: false);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        var memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId);

        var allMemberProperties = chapterProperties
            .Select(x => memberPropertyDictionary.ContainsKey(x.Id)
                ? memberPropertyDictionary[x.Id]
                : new MemberProperty
                {
                    ChapterPropertyId = x.Id,
                    MemberId = currentMember.Id
                });

        foreach (var chapterProperty in chapterProperties)
        {
            var updateProperty = model.Properties
                ?.FirstOrDefault(x => x.ChapterPropertyId == chapterProperty.Id);
            if (updateProperty == null)
            {
                continue;
            }

            if (!memberPropertyDictionary.TryGetValue(chapterProperty.Id, out var memberProperty))
            {
                memberProperty = new MemberProperty
                {
                    ChapterPropertyId = chapterProperty.Id,
                    MemberId = currentMember.Id,
                };
            }

            memberProperty.Value = updateProperty.Value;
            _unitOfWork.MemberPropertyRepository.Upsert(memberProperty);
        }

        _unitOfWork.MemberRepository.Update(currentMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberCurrency(Guid id, Guid currencyId)
    {
        var (currency, paymentSettings) = await _unitOfWork.RunAsync(
            x => x.CurrencyRepository.GetByIdOrDefault(currencyId),
            x => x.MemberPaymentSettingsRepository.GetByMemberId(id));

        if (currency == null)
        {
            return ServiceResult.Failure("Invalid currency");
        }

        paymentSettings ??= new MemberPaymentSettings();

        paymentSettings.CurrencyId = currencyId;

        if (paymentSettings.MemberId == default)
        {
            paymentSettings.MemberId = id;
            _unitOfWork.MemberPaymentSettingsRepository.Add(paymentSettings);
        }
        else
        {
            _unitOfWork.MemberPaymentSettingsRepository.Update(paymentSettings);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberImage(Guid id, byte[] imageData)
    {
        var (member, image, avatar) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(id),
            x => x.MemberImageRepository.GetByMemberId(id),
            x => x.MemberAvatarRepository.GetByMemberId(id));

        image ??= new MemberImage();

        avatar ??= new MemberAvatar();

        var result = _memberImageService.UpdateMemberImage(image, avatar, imageData);
        if (!result.Success)
        {
            return result;
        }

        if (image.MemberId == Guid.Empty)
        {
            image.MemberId = member.Id;
            _unitOfWork.MemberImageRepository.Add(image);
        }
        else
        {
            _unitOfWork.MemberImageRepository.Update(image);
        }

        if (avatar.MemberId == Guid.Empty)
        {
            avatar.MemberId = member.Id;
            _unitOfWork.MemberAvatarRepository.Add(avatar);
        }
        else
        {
            _unitOfWork.MemberAvatarRepository.Update(avatar);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<MemberImage>(id);
        _cacheService.RemoveVersionedItem<MemberAvatar>(id);

        return ServiceResult.Successful("Picture updated");
    }

    public async Task<ServiceResult> UpdateMemberLocation(Guid id, LatLong? location, string? name, Guid? distanceUnitId)
    {
        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(id);

        var memberPreferences = await _unitOfWork.MemberPreferencesRepository.GetByMemberId(id).Run();

        if (location != null && !string.IsNullOrEmpty(name))
        {
            memberLocation ??= new MemberLocation();

            var country = await _geolocationService.GetCountryFromLocation(location.Value);

            memberLocation.CountryId = country?.Id;
            memberLocation.LatLong = location.Value;
            memberLocation.Name = name;
        }
        else
        {
            if (memberLocation == null)
            {
                return ServiceResult.Successful();
            }

            _unitOfWork.MemberLocationRepository.Delete(memberLocation);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.Successful();
        }

        if (memberLocation.MemberId == default)
        {
            memberLocation.MemberId = id;
            _unitOfWork.MemberLocationRepository.Add(memberLocation);
        }
        else
        {
            _unitOfWork.MemberLocationRepository.Update(memberLocation);
        }

        if (memberPreferences?.DistanceUnitId != distanceUnitId)
        {
            memberPreferences ??= new MemberPreferences();

            memberPreferences.DistanceUnitId = distanceUnitId;

            if (memberPreferences.MemberId == default)
            {
                memberPreferences.MemberId = id;
                _unitOfWork.MemberPreferencesRepository.Add(memberPreferences);
            }
            else
            {
                _unitOfWork.MemberPreferencesRepository.Update(memberPreferences);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberSiteProfile(
        IMemberServiceRequest request, MemberSiteProfileUpdateModel model)
    {
        var member = request.CurrentMember;

        member.FirstName = model.FirstName.Trim();
        member.LastName = model.LastName.Trim();

        _unitOfWork.MemberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberTopics(
        IMemberServiceRequest request,
        IReadOnlyCollection<Guid> topicIds,
        IReadOnlyCollection<NewTopicModel> newTopics)
    {
        var currentMemberId = request.CurrentMember.Id;

        var existing = await _unitOfWork.MemberTopicRepository.GetByMemberId(currentMemberId).Run();

        if (_unitOfWork.MemberTopicRepository.Merge(existing, currentMemberId, topicIds) > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        await _topicService.AddNewMemberTopics(
            request,
            newTopics);

        return ServiceResult.Successful();
    }

    private static ServiceResult ChapterIsOpenForRegistration(
        PlatformType platform,
        int members,
        MemberSiteSubscription? ownerSubscription)
    {
        if (ownerSubscription?.SiteSubscription.HasCapacity(members) == true)
        {
            return ServiceResult.Successful();
        }

        return ServiceResult.Failure("This group is not able to welcome any new members");
    }

    private static IEnumerable<string> GetMissingMemberProfileProperties(
        IEnumerable<ChapterProperty> chapterProperties,
        IEnumerable<MemberPropertyUpdateModel> memberProperties,
        bool forApplication)
    {
        var memberPropertyDictionary = memberProperties
            .ToDictionary(x => x.ChapterPropertyId, x => x.Value);
        foreach (var chapterProperty in chapterProperties.Where(x => x.Required))
        {
            memberPropertyDictionary.TryGetValue(chapterProperty.Id, out string? value);

            if (chapterProperty.DataType == DataType.Checkbox)
            {
                if (string.Equals(value, "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (chapterProperty.ApplicationOnly && !forApplication)
            {
                continue;
            }

            yield return !string.IsNullOrEmpty(chapterProperty.DisplayName)
                    ? chapterProperty.DisplayName
                    : chapterProperty.Label;
        }
    }

    private MemberChapter AddMemberToChapter(
        DateTime now,
        Member member,
        Chapter chapter,
        IEnumerable<MemberProperty> memberProperties,
        ChapterMembershipSettings? membershipSettings,
        MemberSiteSubscription? ownerSubscription)
    {
        var memberChapter = new MemberChapter
        {
            Approved = ownerSubscription?.HasFeature(SiteFeatureType.ApproveMembers) != true ||
                membershipSettings?.ApproveNewMembers != true,
            CreatedUtc = now,
            MemberId = member.Id,
            ChapterId = chapter.Id
        };

        _unitOfWork.MemberChapterRepository.Add(memberChapter);

        var hasSubscriptions = _authorizationService
            .ChapterHasAccess(ownerSubscription, SiteFeatureType.MemberSubscriptions);
        if (hasSubscriptions && membershipSettings?.Enabled == true)
        {
            _unitOfWork.MemberSubscriptionRepository.Add(new MemberSubscription
            {
                ExpiresUtc = membershipSettings?.TrialPeriodMonths > 0 ? now.AddMonths(membershipSettings.TrialPeriodMonths) : null,
                MemberChapterId = memberChapter.Id,
                Type = membershipSettings?.TrialPeriodMonths > 0 ? SubscriptionType.Trial : SubscriptionType.Free
            });
        }

        _unitOfWork.MemberPropertyRepository.AddMany(memberProperties);

        return memberChapter;
    }

    private async Task<ServiceResult> CancelSubscription(MemberSubscriptionRecord memberSubscriptionRecord)
    {
        if (memberSubscriptionRecord.ChapterSubscriptionId == null ||
            string.IsNullOrEmpty(memberSubscriptionRecord.ExternalId))
        {
            return ServiceResult.Failure("Error cancelling subscription");
        }

        var (chapterSubscription, sitePaymentSettings, connectedAccount) = await _unitOfWork.RunAsync(
            x => x.ChapterSubscriptionRepository.GetById(memberSubscriptionRecord.ChapterSubscriptionId.Value),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(memberSubscriptionRecord.ChapterId));

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            sitePaymentSettings,
            connectedAccount);

        var success = await paymentProvider.CancelSubscription(memberSubscriptionRecord.ExternalId);
        if (success)
        {
            memberSubscriptionRecord.CancelledUtc = DateTime.UtcNow;
            _unitOfWork.MemberSubscriptionRecordRepository.Update(memberSubscriptionRecord);
            await _unitOfWork.SaveChangesAsync();
        }

        return success
            ? ServiceResult.Successful("Subscription cancelled")
            : ServiceResult.Failure("Error cancelling subscription");
    }

    private async Task<ServiceResult> RequestMemberEmailAddressUpdate(
        IServiceRequest request,
        Chapter? chapter,
        Member member,
        string newEmailAddress,
        MemberEmailAddressUpdateToken? existingToken)
    {
        if (member.EmailAddress.Equals(newEmailAddress, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Successful("New email address matches old email address");
        }

        if (!MailUtils.ValidEmailAddress(newEmailAddress))
        {
            return ServiceResult.Failure("Invalid email address format");
        }

        if (existingToken != null)
        {
            _unitOfWork.MemberEmailAddressUpdateTokenRepository.Delete(existingToken);
        }

        var activationToken = TokenGenerator.GenerateBase64Token(64);

        _unitOfWork.MemberEmailAddressUpdateTokenRepository.Add(new MemberEmailAddressUpdateToken
        {
            ConfirmationToken = activationToken,
            MemberId = member.Id,
            NewEmailAddress = newEmailAddress
        });

        await _memberEmailService.SendAddressUpdateEmail(
            request,
            chapter,
            member,
            newEmailAddress,
            activationToken);

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateMemberProfile(
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        MemberChapterProfileUpdateModel profile,
        bool forApplication)
    {
        var missingProperties = GetMissingMemberProfileProperties(chapterProperties, profile.Properties, forApplication).ToArray();
        if (missingProperties.Length > 0)
        {
            return ServiceResult.Failure($"The following properties are required: {string.Join(", ", missingProperties)}");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateMemberProfile(
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        MemberCreateProfile profile,
        bool forApplication)
    {
        var propertyResult = ValidateMemberProperties(chapterProperties, profile.Properties, forApplication);

        if (!MailUtils.ValidEmailAddress(profile.EmailAddress))
        {
            return ServiceResult.Failure("Invalid email address format");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateMemberProperties(
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IEnumerable<MemberPropertyUpdateModel> memberProperties,
        bool forApplication)
    {
        var missingProperties = GetMissingMemberProfileProperties(chapterProperties, memberProperties, forApplication)
            .ToArray();
        if (missingProperties.Length > 0)
        {
            return ServiceResult.Failure($"The following properties are required: {string.Join(", ", missingProperties)}");
        }

        return ServiceResult.Successful();
    }
}