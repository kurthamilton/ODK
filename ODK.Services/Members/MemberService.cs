﻿using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Cryptography;
using ODK.Core.DataTypes;
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Topics;
using ODK.Services.Topics.Models;

namespace ODK.Services.Members;

public class MemberService : IMemberService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberImageService _memberImageService;
    private readonly INotificationService _notificationService;
    private readonly IOAuthProviderFactory _oauthProviderFactory;
    private readonly IPaymentService _paymentService;    
    private readonly IPlatformProvider _platformProvider;
    private readonly ITopicService _topicService;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(
        IUnitOfWork unitOfWork, 
        IAuthorizationService authorizationService,
        IPaymentService paymentService, 
        ICacheService cacheService, 
        IMemberImageService memberImageService, 
        IPlatformProvider platformProvider, 
        IMemberEmailService memberEmailService,
        INotificationService notificationService,
        IOAuthProviderFactory oauthProviderFactory,
        ITopicService topicService)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _memberEmailService = memberEmailService;
        _memberImageService = memberImageService;
        _notificationService = notificationService;
        _oauthProviderFactory = oauthProviderFactory;
        _paymentService = paymentService;        
        _platformProvider = platformProvider;
        _topicService = topicService;
        _unitOfWork = unitOfWork;
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
    
    public async Task<ServiceResult<Member?>> CreateAccount(CreateAccountModel model)
    {
        var platform = _platformProvider.GetPlatform();
        var (existing, siteSubscription, distanceUnits, topics) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByEmailAddress(model.EmailAddress),
            x => x.SiteSubscriptionRepository.GetDefault(platform),
            x => x.DistanceUnitRepository.GetAll(),
            x => x.TopicRepository.GetByIds(model.TopicIds));

        if (existing != null)
        {
            await _memberEmailService.SendDuplicateMemberEmail(null, existing);
            return ServiceResult<Member?>.Successful(null);
        }
        
        TimeZoneInfo? timeZone = null;
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(model.TimeZoneId, out timeZone))
        {
            return ServiceResult<Member?>.Failure("Invalid time zone");
        }

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
            _unitOfWork.MemberLocationRepository.Add(new MemberLocation
            {
                MemberId = member.Id,
                LatLong = model.Location.Value,
                Name = model.LocationName
            });
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
            activationToken = RandomStringGenerator.Generate(64);
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

        await _topicService.AddNewMemberTopics(member.Id, model.NewTopics);

        if (!string.IsNullOrEmpty(activationToken))
        {
            await _memberEmailService.SendActivationEmail(null, member, activationToken);
        }
        else
        {
            await _memberEmailService.SendSiteWelcomeEmail(member);
        }

        return ServiceResult<Member?>.Successful(member);
    }

    public async Task<ServiceResult> CreateChapterAccount(Guid chapterId, CreateMemberProfile model)
    {
        var platform = _platformProvider.GetPlatform();
        var (
            chapter, 
            chapterProperties, 
            membershipSettings, 
            existing, 
            siteSettings, 
            siteSubscription, 
            ownerSubscription
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetByEmailAddress(model.EmailAddress),
            x => x.SiteSettingsRepository.Get(),
            x => x.SiteSubscriptionRepository.GetDefault(platform),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId));

        var chapterLocation = await _unitOfWork.ChapterLocationRepository.GetByChapterId(chapterId);
        
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
            await _memberEmailService.SendDuplicateMemberEmail(chapter, existing);
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
            SuperAdmin = false,
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

        if (chapterLocation != null)
        {
            _unitOfWork.MemberLocationRepository.Add(new MemberLocation
            {
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

        var activationToken = RandomStringGenerator.Generate(64);
        _unitOfWork.MemberActivationTokenRepository.Add(new MemberActivationToken
        {
            ActivationToken = activationToken,
            ChapterId = chapterId,
            MemberId = member.Id
        });        

        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _memberEmailService.SendActivationEmail(chapter, member, activationToken);

            return ServiceResult.Successful();
        }        
        catch
        {
            return ServiceResult.Failure("Your account has been created but an error occurred sending an email.");
        }
    }

    public async Task<ServiceResult> DeleteMember(Guid memberId)
    {
        var (member, chapters) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterRepository.GetByOwnerId(memberId));

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

        _unitOfWork.MemberRepository.Delete(member);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult<(Member Member, MemberChapter MemberChapter)>> DeleteMemberChapterData(Guid memberId, Guid chapterId)
    {
        var (chapter, chapterAdminMembers, member, memberProperties, notifications) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberPropertyRepository.GetByMemberId(memberId, chapterId),
            x => x.NotificationRepository.GetByMemberId(memberId, chapterId));

        if (chapter.OwnerId == memberId)
        {
            return ServiceResult<(Member, MemberChapter)>.Failure("Group owners cannot leave their own groups");
        }

        var memberChapter = member.MemberChapter(chapterId);
        if (memberChapter == null)
        {
            return ServiceResult<(Member, MemberChapter)>.Failure("Member is not a member of this group");
        }

        member.Chapters.Remove(memberChapter);
        _unitOfWork.MemberChapterRepository.Delete(memberChapter);

        var chapterAdminMember = chapterAdminMembers
            .FirstOrDefault(x => x.MemberId == memberId);
        if (chapterAdminMember != null)
        {
            _unitOfWork.ChapterAdminMemberRepository.Delete(chapterAdminMember);
        }

        var privacySettings = member.PrivacySettings
            .FirstOrDefault(x => x.ChapterId == chapter.Id);

        if (privacySettings != null)
        {
            _unitOfWork.MemberPrivacySettingsRepository.Delete(privacySettings);
        }

        _unitOfWork.MemberPropertyRepository.DeleteMany(memberProperties);
        _unitOfWork.NotificationRepository.DeleteMany(notifications);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<(Member, MemberChapter)>.Successful((member, memberChapter));
    }

    public async Task<Member?> FindMemberByEmailAddress(string emailAddress)
    {
        return await _unitOfWork.MemberRepository.GetByEmailAddress(emailAddress).Run();
    }

    public async Task<Member> GetMember(Guid memberId)
    {
        var member = await _unitOfWork.MemberRepository.GetById(memberId).Run();
        return member;
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

    public async Task<MemberLocation?> GetMemberLocation(Guid memberId)
    {
        return await _unitOfWork.MemberLocationRepository.GetByMemberId(memberId);
    }

    public async Task<MemberPreferences?> GetMemberPreferences(Guid memberId)
    {
        return await _unitOfWork.MemberPreferencesRepository.GetByMemberId(memberId).Run();
    }

    public async Task<ServiceResult> JoinChapter(Guid currentMemberId, Guid chapterId, IEnumerable<UpdateMemberProperty> properties)
    {
        var platform = _platformProvider.GetPlatform();

        var (
            chapter, 
            adminMembers,
            notificationSettings,
            currentMember, 
            ownerSubscription, 
            members, 
            chapterProperties, 
            chapterPropertyOptions, 
            membershipSettings
            ) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapterId, NotificationType.NewMember),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetCountByChapterId(chapterId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.ChapterPropertyOptionRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

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

        await _memberEmailService.SendNewMemberAdminEmail(chapter, adminMembers, currentMember, chapterProperties, memberProperties);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> LeaveChapter(Guid currentMemberId, Guid chapterId, string reason)
    {
        var (chapter, adminMembers) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId));        

        var result = await DeleteMemberChapterData(currentMemberId, chapterId);
        var (currentMember, memberChapter) = result.Value;
        if (currentMember == null)
        {
            return result;
        }

        await _memberEmailService.SendMemberLeftChapterEmail(
            chapter,
            adminMembers,
            currentMember,
            reason);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> PurchaseChapterSubscription(Guid memberId, Guid chapterId, Guid chapterSubscriptionId,
        string cardToken)
    {
        var (paymentSettings, member, chapterSubscription, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterSubscriptionRepository.GetByIdOrDefault(chapterSubscriptionId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId));
        if (chapterSubscription == null || chapterSubscription.ChapterId != chapterId)
        {
            return ServiceResult.Failure("Payment not made: subscription not found");
        }

        var memberChapter = member.MemberChapter(chapterSubscription.ChapterId);
        if (memberChapter == null)
        {
            return ServiceResult.Failure("Payment not made: you are not a member of this subscription's chapter");
        }

        var paymentResult = await _paymentService.MakePayment(paymentSettings, 
            paymentSettings.Currency, member, (decimal)chapterSubscription.Amount, cardToken, 
            chapterSubscription.Title);
        if (!paymentResult.Success)
        {
            return ServiceResult.Failure($"Payment not made: {paymentResult.Message}");
        }

        memberSubscription ??= new MemberSubscription();

        var now = memberSubscription.ExpiresUtc > DateTime.UtcNow ? memberSubscription.ExpiresUtc.Value : DateTime.UtcNow;
        var expiresUtc = now.AddMonths(chapterSubscription.Months);
        memberSubscription.ExpiresUtc = expiresUtc;
        memberSubscription.Type = chapterSubscription.Type;

        if (memberSubscription.MemberChapterId == default)
        {
            memberSubscription.MemberChapterId = memberChapter.Id;
            _unitOfWork.MemberSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSubscriptionRepository.Update(memberSubscription);
        }        

        _unitOfWork.MemberSubscriptionRepository.AddMemberSubscriptionRecord(new MemberSubscriptionRecord
        {
            Amount = chapterSubscription.Amount,
            ChapterId = chapterId,
            MemberId = memberId,
            Months = chapterSubscription.Months,
            PurchasedUtc = DateTime.UtcNow,
            Type = chapterSubscription.Type
        });

        await _unitOfWork.SaveChangesAsync();

        var (chapter, chapterPaymentSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterSubscription.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterSubscription.ChapterId));

        await _memberEmailService.SendMemberChapterSubscriptionConfirmationEmail(
            chapter,
            chapterPaymentSettings,
            chapterSubscription,
            member,
            expiresUtc);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, Guid chapterId, string newEmailAddress)
    {
        var (chapter, member, existingToken) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberEmailAddressUpdateTokenRepository.GetByMemberId(memberId));

        return await RequestMemberEmailAddressUpdate(chapter, member, newEmailAddress, existingToken);
    }

    public async Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, string newEmailAddress)
    {
        var (member, existingToken) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberEmailAddressUpdateTokenRepository.GetByMemberId(memberId));
        return await RequestMemberEmailAddressUpdate(null, member, newEmailAddress, existingToken);
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

    public async Task<bool> CompleteChapterSubscriptionCheckoutSession(
        Guid memberId, Guid chapterSubscriptionId, string sessionId)
    {
        var platform = _platformProvider.GetPlatform();

        var (member, chapterSubscription, paymentCheckoutSession) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterSubscriptionRepository.GetById(chapterSubscriptionId),
            x => x.PaymentCheckoutSessionRepository.GetByMemberId(memberId, sessionId));

        if (paymentCheckoutSession == null || paymentCheckoutSession.CompletedUtc != null)
        {
            return false;
        }

        var memberChapter = member.MemberChapter(chapterSubscription.ChapterId);
        OdkAssertions.Exists(memberChapter);

        var (chapterPaymentSettings, sitePaymentSettings, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterSubscription.ChapterId),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterSubscription.ChapterId));

        IPaymentSettings paymentSettings = chapterPaymentSettings.UseSitePaymentProvider
            ? sitePaymentSettings
            : chapterPaymentSettings;

        if (string.IsNullOrEmpty(chapterSubscription.ExternalId))
        {
            throw new Exception("Error completing checkout session: chapterSubscription.ExternalId missing");
        }

        var subscriptionPlan = await _paymentService.GetSubscriptionPlan(paymentSettings, chapterSubscription.ExternalId);
        if (subscriptionPlan == null)
        {
            throw new Exception("Error completing checkout session: subscriptionPlan not found");
        }

        var checkoutSession = await _paymentService.GetCheckoutSession(paymentSettings, sessionId);
        if (checkoutSession?.Complete != true)
        {
            return false;
        }

        memberSubscription ??= new MemberSubscription();

        var now = memberSubscription.ExpiresUtc > DateTime.UtcNow ? memberSubscription.ExpiresUtc.Value : DateTime.UtcNow;
        var expiresUtc = now.AddMonths(chapterSubscription.Months);
        memberSubscription.ExpiresUtc = expiresUtc;
        memberSubscription.Type = chapterSubscription.Type;

        if (memberSubscription.MemberChapterId == default)
        {
            memberSubscription.MemberChapterId = memberChapter.Id;
            _unitOfWork.MemberSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSubscriptionRepository.Update(memberSubscription);
        }

        var memberSubscriptionRecord = new MemberSubscriptionRecord
        {
            Amount = chapterSubscription.Amount,
            ChapterId = chapterSubscription.ChapterId,
            MemberId = memberId,
            Months = chapterSubscription.Months,
            PaymentId = Guid.NewGuid(),
            PurchasedUtc = DateTime.UtcNow,
            Type = chapterSubscription.Type
        };
        _unitOfWork.MemberSubscriptionRepository.AddMemberSubscriptionRecord(memberSubscriptionRecord);

        paymentCheckoutSession.CompletedUtc = DateTime.UtcNow;
        _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);

        var payment = new Payment
        {
            Amount = (decimal)memberSubscriptionRecord.Amount,
            ChapterId = memberSubscriptionRecord.ChapterId,
            CurrencyId = chapterPaymentSettings.CurrencyId,
            Id = memberSubscriptionRecord.PaymentId.Value,
            MemberId = memberSubscriptionRecord.MemberId,
            PaidUtc = memberSubscriptionRecord.PurchasedUtc,
            Reference = $"Subscription: {chapterSubscription.Name}"
        };

        _unitOfWork.PaymentRepository.Add(payment);

        await _unitOfWork.SaveChangesAsync();

        if (chapterPaymentSettings?.UseSitePaymentProvider == true)
        {
            var siteEmailSettings = await _unitOfWork.SiteEmailSettingsRepository.Get(platform).Run();
            var currency = chapterPaymentSettings.Currency;

            await _memberEmailService.SendPaymentNotification(payment, currency, siteEmailSettings);
        }

        return true;
    }

    public async Task<ChapterSubscriptionCheckoutViewModel> StartChapterSubscriptionCheckoutSession(
        Guid memberId, Guid chapterSubscriptionId, string returnPath)
    {
        var platform = _platformProvider.GetPlatform();

        var (member, chapterSubscription) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterSubscriptionRepository.GetById(chapterSubscriptionId));

        var (chapterPaymentSettings, sitePaymentSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterSubscription.ChapterId),
            x => x.SitePaymentSettingsRepository.GetActive());

        IPaymentSettings paymentSettings = chapterPaymentSettings.UseSitePaymentProvider
            ? sitePaymentSettings
            : chapterPaymentSettings;        

        if (string.IsNullOrEmpty(chapterSubscription.ExternalId))
        {
            throw new Exception("Error starting checkout session: chapterSubscription.ExternalId missing");
        }

        var subscriptionPlan = await _paymentService.GetSubscriptionPlan(paymentSettings, chapterSubscription.ExternalId);
        if (subscriptionPlan == null)
        {
            throw new Exception("Error starting checkout session: subscriptionPlan not found");
        }

        var session = await _paymentService.StartCheckoutSession(paymentSettings, subscriptionPlan, returnPath);

        _unitOfWork.PaymentCheckoutSessionRepository.Add(new PaymentCheckoutSession
        {
            MemberId = memberId,
            PaymentId = chapterSubscriptionId,
            SessionId = session.SessionId,
            StartedUtc = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        return new ChapterSubscriptionCheckoutViewModel
        {
            ClientSecret = session.ClientSecret,
            CurrencyCode = subscriptionPlan.CurrencyCode,
            PaymentSettings = paymentSettings,
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

    public async Task<ServiceResult> UpdateMemberChapterProfile(Guid id, Guid chapterId, UpdateMemberChapterProfile model)
    {
        var (chapterProperties, member, memberProperties) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(id),
            x => x.MemberPropertyRepository.GetByMemberId(id, chapterId));

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
                    MemberId = member.Id
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
                    MemberId = member.Id,                    
                };
            }            

            memberProperty.Value = updateProperty.Value;
            _unitOfWork.MemberPropertyRepository.Upsert(memberProperty);
        }

        _unitOfWork.MemberRepository.Update(member);
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

    public async Task<ServiceResult> UpdateMemberSiteProfile(Guid id, UpdateMemberSiteProfile model)
    {
        var member = await _unitOfWork.MemberRepository.GetById(id).Run();

        member.FirstName = model.FirstName.Trim();
        member.LastName = model.LastName.Trim();

        _unitOfWork.MemberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberTopics(
        Guid id, 
        IReadOnlyCollection<Guid> topicIds, 
        IReadOnlyCollection<NewTopicModel> newTopics)
    {
        var existing = await _unitOfWork.MemberTopicRepository.GetByMemberId(id).Run();

        if (_unitOfWork.MemberTopicRepository.Merge(existing, id, topicIds) > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        await _topicService.AddNewMemberTopics(id, newTopics);

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
        CreateMemberProfile profile, 
        IEnumerable<ChapterProperty> chapterProperties,
        IEnumerable<UpdateMemberProperty> memberProperties,
        bool forApplication)
    {
        if (string.IsNullOrWhiteSpace(profile.EmailAddress))
        {
            yield return "Email address";
        }

        var missingProperties = GetMissingMemberProfileProperties(chapterProperties, memberProperties, forApplication);
        foreach (string property in missingProperties)
        {
            yield return property;
        }
    }

    private static IEnumerable<string> GetMissingMemberProfileProperties(
        IEnumerable<ChapterProperty> chapterProperties,
        IEnumerable<UpdateMemberProperty> memberProperties,
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

    private async Task<ServiceResult> RequestMemberEmailAddressUpdate(
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

        var activationToken = RandomStringGenerator.Generate(64);

        _unitOfWork.MemberEmailAddressUpdateTokenRepository.Add(new MemberEmailAddressUpdateToken
        {
            ConfirmationToken = activationToken,
            MemberId = member.Id,
            NewEmailAddress = newEmailAddress
        });

        await _memberEmailService.SendAddressUpdateEmail(chapter, member, newEmailAddress, activationToken);

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateMemberProfile(
        IReadOnlyCollection<ChapterProperty> chapterProperties, 
        UpdateMemberChapterProfile profile,
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
        CreateMemberProfile profile,
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
        IEnumerable<UpdateMemberProperty> memberProperties,
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
