using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Cryptography;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Data.Core.Countries;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Geolocation;
using ODK.Services.Logging;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.ChapterMembership;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Topics;
using ODK.Services.Topics.Models;
using ODK.Services.Workflows;

namespace ODK.Services.Members;

public class MemberService : IMemberService
{
    private readonly StateMachineRunner<AccountState, AccountTrigger, AccountContext> _accountWorkflow;
    private readonly IAccountContextFactory _accountContextFactory;
    private readonly IChapterMembershipContextFactory _chapterMembershipContextFactory;
    private readonly StateMachineRunner<
        ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext> _chapterMembershipWorkflow;
    private readonly IEmailValidationService _emailValidationService;
    private readonly IDistanceUnitFactory _distanceUnitFactory;
    private readonly IGeolocationService _geolocationService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberImageService _memberImageService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ITopicService _topicService;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(
        IUnitOfWork unitOfWork,
        IMemberImageService memberImageService,
        IMemberEmailService memberEmailService,
        ITopicService topicService,
        IPaymentProviderFactory paymentProviderFactory,
        IGeolocationService geolocationService,
        ILoggingService loggingService,
        IDistanceUnitFactory distanceUnitFactory,
        IEmailValidationService emailValidationService,
        IChapterMembershipContextFactory chapterMembershipContextFactory,
        StateMachineRunner<ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>
            chapterMembershipWorkflow,
        StateMachineRunner<AccountState, AccountTrigger, AccountContext> accountWorkflow,
        IAccountContextFactory accountContextFactory)
    {
        _accountWorkflow = accountWorkflow;
        _accountContextFactory = accountContextFactory;
        _chapterMembershipWorkflow = chapterMembershipWorkflow;
        _chapterMembershipContextFactory = chapterMembershipContextFactory;
        _emailValidationService = emailValidationService;
        _distanceUnitFactory = distanceUnitFactory;
        _geolocationService = geolocationService;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _memberImageService = memberImageService;
        _paymentProviderFactory = paymentProviderFactory;
        _topicService = topicService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CancelChapterSubscription(Guid memberId, string externalId)
    {
        var (member, memberSubscriptionRecord) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .ForExternalId(externalId)
                .OrderByDescending(x => x.PurchasedUtc)
                .GetSingleOrDefault());

        if (memberSubscriptionRecord == null || memberSubscriptionRecord.MemberId != member.Id)
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
            await _unitOfWork.SaveChanges();
            return ServiceResult.Failure("Email not updated: new email address is already in use");
        }

        member.EmailAddress = token.NewEmailAddress;
        _unitOfWork.MemberRepository.Update(member);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult<Member?>> CreateAccount(IServiceRequest request, AccountCreateModel model)
    {
        var context = await _accountContextFactory.CreateForSiteSignUp(request, model);

        var result = await _accountWorkflow.Fire(AccountTrigger.SignUp, context);
        if (!result.Success)
        {
            return ServiceResult<Member?>.Failure(result.Message ?? string.Empty);
        }

        /* Null when the address already had an account it could sign in with: nothing was created and the
           address was emailed to say so. The caller reports that identically to a genuine sign-up, so nobody
           can find out from the response whether an address is registered. */
        return ServiceResult<Member?>.Successful(context.NewMember);
    }

    public async Task<CreateChapterAccountResult> CreateChapterAccount(
        IChapterServiceRequest request, MemberCreateProfile model)
    {
        await _loggingService.Info($"Creating chapter account for {model.EmailAddress}");

        var context = await _accountContextFactory.CreateForGroupSignUp(request, model);

        var result = await _accountWorkflow.Fire(AccountTrigger.SignUp, context);
        if (!result.Success)
        {
            return CreateChapterAccountResult.FromResult(result.ToServiceResult());
        }

        /* Holding the token from an invitation sent to the address being registered proves the sign-up reached
           that inbox, which is all an activation email establishes - so no email was sent and the caller hands
           them straight to setting a password. Read from the same rule the machine picked the edge with. */
        return context.PresentedTheInviteToken
            ? CreateChapterAccountResult.SuccessfulReadyToActivate(context.RequiredActivationToken)
            : CreateChapterAccountResult.Successful();
    }

    public async Task<ServiceResult> DeleteMember(IMemberServiceRequest request)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var chapters = await _unitOfWork.ChapterRepository
            .GetByOwnerId(platform, currentMember.Id)
            .Run();

        if (chapters.Count > 0)
        {
            return ServiceResult.Failure("Your groups must be transferred or deleted before deleting your account");
        }

        _unitOfWork.MemberRepository.Delete(currentMember);
        await _unitOfWork.SaveChanges();

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

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<Member?> FindMemberByEmailAddress(string emailAddress)
    {
        return await _unitOfWork.MemberRepository.GetByEmailAddress(emailAddress).Run();
    }

    public async Task<LocationDefaultsViewModel> GetLocationDefaults(LatLong location)
    {
        var country = await _geolocationService.GetCountryFromLocation(location);

        var currency = country != null
            ? await _unitOfWork.CurrencyRepository.GetByCountryId(country.Id).Run()
            : null;

        var currencyDto = currency != null && country != null
            ? new CurrencyDto
            {
                CountryIsoCode2 = currency.CountryIsoCode2 ?? country.IsoCode2,
                CountryIsoCode3 = currency.CountryIsoCode3 ?? country.IsoCode3,
                CountryName = currency.CountryName ?? country.Name,
                Currency = currency
            }
            : null;

        var distanceUnit = country != null
            ? _distanceUnitFactory.Get(country.DistanceUnit)
            : null;

        return new LocationDefaultsViewModel
        {
            Country = country,
            Currency = currencyDto,
            DistanceUnit = distanceUnit,
            TimeZone = await _geolocationService.GetTimeZoneFromLocation(location)
        };
    }

    public async Task<MemberAvatar> GetMemberAvatar(Guid memberId)
    {
        var avatar = await _unitOfWork.MemberAvatarRepository.GetByMemberId(memberId).Run();
        OdkAssertions.Exists(avatar);
        return avatar;
    }

    public async Task<MemberLocationViewModel> GetMemberLocationViewModel(IMemberServiceRequest request)
    {
        var currentMember = request.CurrentMember;

        var (memberPreferences, memberLocation) = await _unitOfWork.RunAsync(
            x => x.MemberPreferencesRepository.GetByMemberIdOrDefault(currentMember.Id),
            x => x.MemberLocationRepository.GetByMemberIdOrDefault(currentMember.Id));

        var distanceUnits = _distanceUnitFactory.GetAll();

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
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForMember(memberId)
                .ForChapter(chapterId)
                .ToChapterSubscription()
                .GetSingleOrDefault(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        return new MemberSubscriptionAlertViewModel
        {
            ChapterMembershipSettings = chapterMembershipSettings,
            HasActiveRecurringSubscription = memberSubscription?.IsActiveRecurring() == true,
            MemberSubscription = memberSubscription
        };
    }

    public async Task<ServiceResult> JoinChapter(
        IMemberChapterServiceRequest request, IEnumerable<MemberPropertyUpdateModel> properties)
    {
        var context = await _chapterMembershipContextFactory.CreateForJoin(request, properties);
        var result = await _chapterMembershipWorkflow.Fire(ChapterMembershipTrigger.Join, context);

        /* Already being in the group is not checked here: the machine has no Join edge out of a state that
           holds a membership, so it reports the trigger as not permitted from there. Only the wording is
           this method's. */
        if (!result.Success &&
            result.From is ChapterMembershipState.Joined or ChapterMembershipState.PendingApproval)
        {
            return ServiceResult.Failure("You are already a member of this group");
        }

        return result.ToServiceResult();
    }

    public async Task<ServiceResult> LeaveChapter(IMemberChapterServiceRequest request, string reason)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (adminMembers, subscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .ForMember(currentMember.Id)
                .ForChapter(chapter.Id)
                .OrderByDescending(x => x.PurchasedUtc)
                .GetSingleOrDefault());

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
        var (member, avatar) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberAvatarRepository.GetByMemberId(memberId));

        if (avatar == null)
        {
            return;
        }

        avatar ??= new MemberAvatar();

        _memberImageService.RotateMemberImage(avatar);

        _unitOfWork.MemberAvatarRepository.Update(avatar);

        await _unitOfWork.SaveChanges();
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
        var paymentCheckoutSessionId = _unitOfWork.NewId();
        var paymentId = _unitOfWork.NewId();

        var metadata = new PaymentMetadataModel(
            platform,
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
            Amount = chapterSubscription.Amount,
            ChapterId = chapterSubscription.ChapterId,
            CreatedUtc = utcNow,
            CurrencyId = chapterSubscription.Currency.Id,
            ExternalId = externalCheckoutSession.PaymentId,
            Id = paymentId,
            MemberId = currentMember.Id,
            Reference = chapterSubscription.ToReference(),
            SitePaymentSettingId = chapterSubscription.SitePaymentSettingId
        });

        await _unitOfWork.SaveChanges();

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

        await _unitOfWork.SaveChanges();

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
        await _unitOfWork.SaveChanges();

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

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberImage(Guid id, byte[] imageData)
    {
        var (member, avatar) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(id),
            x => x.MemberAvatarRepository.GetByMemberId(id));

        avatar ??= new MemberAvatar();

        var result = _memberImageService.UpdateMemberImage(avatar, imageData);
        if (!result.Success)
        {
            return result;
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

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful("Picture updated");
    }

    public async Task<ServiceResult> UpdateMemberLocation(Guid id, LatLong? location, string? name, DistanceUnitType? distanceUnit)
    {
        var (memberLocation, memberPreferences) = await _unitOfWork.RunAsync(
            x => x.MemberLocationRepository.GetByMemberIdOrDefault(id),
            x => x.MemberPreferencesRepository.GetByMemberIdOrDefault(id));

        if (location != null && !string.IsNullOrEmpty(name))
        {
            memberLocation ??= new MemberLocation();

            var country = await _geolocationService.GetCountryFromLocation(location.Value);

            memberLocation.CountryId = country?.Id;
            memberLocation.Latitude = location.Value.Lat;
            memberLocation.Longitude = location.Value.Long;
            memberLocation.Name = name;
        }
        else
        {
            if (memberLocation == null)
            {
                return ServiceResult.Successful();
            }

            _unitOfWork.MemberLocationRepository.Delete(memberLocation);
            await _unitOfWork.SaveChanges();
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

        if (memberPreferences?.DistanceUnit != distanceUnit)
        {
            memberPreferences ??= new MemberPreferences();

            memberPreferences.DistanceUnit = distanceUnit;

            _unitOfWork.MemberPreferencesRepository.Upsert(memberPreferences, id);
        }

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberSiteProfile(
        IMemberServiceRequest request, MemberSiteProfileUpdateModel model)
    {
        var member = request.CurrentMember;

        member.FirstName = model.FirstName.Trim();
        member.LastName = model.LastName.Trim();

        _unitOfWork.MemberRepository.Update(member);
        await _unitOfWork.SaveChanges();

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
            await _unitOfWork.SaveChanges();
        }

        await _topicService.AddNewMemberTopics(
            request,
            newTopics);

        return ServiceResult.Successful();
    }

    private static IEnumerable<string> GetMissingMemberProfileProperties(
        IEnumerable<ChapterProperty> chapterProperties,
        IEnumerable<MemberPropertyUpdateModel> memberProperties,
        bool forApplication)
    {
        var valuesByPropertyId = memberProperties
            .ToDictionary(x => x.ChapterPropertyId, x => (string?)x.Value);

        return chapterProperties
            .GetMissingRequired(valuesByPropertyId, forApplication)
            .Select(x => x.GetDisplayText());
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
            await _unitOfWork.SaveChanges();
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

        var emailValidationResult = await _emailValidationService.Validate(newEmailAddress, EmailValidationLevel.Full);
        if (!emailValidationResult.Success)
        {
            return emailValidationResult;
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

}
