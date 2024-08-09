using System.Web;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Cryptography;
using ODK.Core.Emails;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Emails;
using ODK.Services.Payments;
using ODK.Services.Platforms;

namespace ODK.Services.Members;

public class MemberService : IMemberService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IChapterUrlService _chapterUrlService;
    private readonly IEmailService _emailService;
    private readonly IMemberImageService _memberImageService;
    private readonly IPaymentService _paymentService;    
    private readonly IPlatformProvider _platformProvider;
    private readonly MemberServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(IUnitOfWork unitOfWork, IAuthorizationService authorizationService,
        IEmailService emailService, MemberServiceSettings settings, IPaymentService paymentService,
        ICacheService cacheService, IMemberImageService memberImageService, 
        IChapterUrlService chapterUrlService, IPlatformProvider platformProvider)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _chapterUrlService = chapterUrlService;
        _emailService = emailService;
        _memberImageService = memberImageService;
        _paymentService = paymentService;        
        _platformProvider = platformProvider;
        _settings = settings;
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

        var existing = await _unitOfWork.MemberRepository.GetByEmailAddress(token.NewEmailAddress).RunAsync();
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
    
    public async Task<ServiceResult> CreateAccount(CreateAccountModel model)
    {
        var platform = _platformProvider.GetPlatform();
        var (existing, siteSubscription) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByEmailAddress(model.EmailAddress),
            x => x.SiteSubscriptionRepository.GetDefault(platform));

        if (existing != null)
        {
            // TODO: send duplicate email
            return ServiceResult.Successful();
        }

        var member = new Member
        {
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = model.EmailAddress,
            FirstName = model.FirstName,
            LastName = model.LastName            
        };
        _unitOfWork.MemberRepository.Add(member);

        _unitOfWork.MemberLocationRepository.Add(new MemberLocation
        {
            MemberId = member.Id,
            LatLong = model.Location,
            Name = model.LocationName
        });

        _unitOfWork.MemberSiteSubscriptionRepository.Add(new MemberSiteSubscription
        {
            MemberId = member.Id,
            SiteSubscriptionId = siteSubscription.Id
        });

        var activationToken = RandomStringGenerator.Generate(64);
        _unitOfWork.MemberActivationTokenRepository.Add(new MemberActivationToken
        {
            ActivationToken = activationToken,
            MemberId = member.Id
        });

        await _unitOfWork.SaveChangesAsync();

        await SendActivationEmailAsync(null, member, activationToken);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateMember(Guid chapterId, CreateMemberProfile model)
    {
        var platform = _platformProvider.GetPlatform();
        var (chapter, chapterProperties, membershipSettings, existing, siteSettings, siteSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetByEmailAddress(model.EmailAddress),
            x => x.SiteSettingsRepository.Get(),
            x => x.SiteSubscriptionRepository.GetDefault(platform));

        var chapterLocation = await _unitOfWork.ChapterLocationRepository.GetByChapterId(chapterId);

        var validationResult = ValidateMemberProfile(chapterProperties, model);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        var image = new MemberImage();
        var avatar = new MemberAvatar();

        var imageValidationResult = _memberImageService.ProcessMemberImage(image, avatar, model.Image, model.ImageCropInfo);
        if (!imageValidationResult.Success)
        {
            return imageValidationResult;
        }

        if (existing != null)
        {
            return ServiceResult.Failure("Email address already in use");
        }

        var now = DateTime.UtcNow;

        var member = new Member
        {
            Activated = false,
            CreatedUtc = now,
            Disabled = false,
            EmailAddress = model.EmailAddress,
            EmailOptIn = model.EmailOptIn ?? false,
            FirstName = model.FirstName,
            LastName = model.LastName,            
            SuperAdmin = false,
            TimeZone = chapter.TimeZone
        };
        
        member.Chapters.Add(new MemberChapter
        {
            CreatedUtc = now,
            MemberId = member.Id,
            ChapterId = chapterId
        });

        _unitOfWork.MemberRepository.Add(member);

        _unitOfWork.MemberLocationRepository.Add(new MemberLocation
        {
            LatLong = chapterLocation?.LatLong,
            Name = chapterLocation?.Name
        });

        _unitOfWork.MemberSiteSubscriptionRepository.Add(new MemberSiteSubscription
        {
            MemberId = member.Id,
            SiteSubscriptionId = siteSubscription.Id
        });

        _unitOfWork.MemberSubscriptionRepository.Add(new MemberSubscription
        {
            ChapterId = chapterId,
            ExpiresUtc = now.AddMonths(membershipSettings?.TrialPeriodMonths ?? siteSettings.DefaultTrialPeriodMonths),
            MemberId = member.Id,
            Type = SubscriptionType.Trial
        });

        image.MemberId = member.Id;
        _unitOfWork.MemberImageRepository.Add(image);

        avatar.MemberId = member.Id;
        _unitOfWork.MemberAvatarRepository.Add(avatar);

        foreach (var property in model.Properties)
        {
            _unitOfWork.MemberPropertyRepository.Add(new MemberProperty
            {
                ChapterPropertyId = property.ChapterPropertyId,
                MemberId = member.Id,
                Value = property.Value
            });
        }

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
            await SendActivationEmailAsync(chapter, member, activationToken);

            return ServiceResult.Successful();
        }        
        catch
        {
            return ServiceResult.Failure("Your account has been created but an error occurred sending an email.");
        }
    }

    public async Task DeleteMember(Guid memberId)
    {
        var member = await _unitOfWork.MemberRepository.GetById(memberId).RunAsync();
        _unitOfWork.MemberRepository.Delete(member);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Member> GetMember(Guid memberId)
    {
        var member = await _unitOfWork.MemberRepository.GetById(memberId).RunAsync();
        return member;
    }

    public async Task<Member> GetMember(Guid memberId, Guid chapterId)
    {
        var member = await _unitOfWork.MemberRepository.GetById(memberId).RunAsync();        
        return OdkAssertions.MeetsCondition(member, 
            x => x.IsMemberOf(chapterId) && member.Visible(chapterId));
    }    

    public async Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId)
    {
        var result = await _cacheService.GetOrSetVersionedItem(
            () => _unitOfWork.MemberImageRepository.GetByMemberId(memberId).RunAsync(),
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
            () => _unitOfWork.MemberAvatarRepository.GetByMemberId(memberId).RunAsync(),
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

    public async Task<MemberProfile?> GetMemberProfile(Guid chapterId, Guid currentMemberId, Member member)
    {
        var (currentMember, chapterProperties, memberProperties) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.MemberPropertyRepository.GetByMemberId(member.Id, chapterId));

        if (member == null || !member.CanBeViewedBy(currentMember))
        {
            return null;
        }

        var memberPropertyDictionary = memberProperties
            .ToDictionary(x => x.ChapterPropertyId);

        var allMemberProperties = chapterProperties
            .Select(x => memberPropertyDictionary.ContainsKey(x.Id)
                ? memberPropertyDictionary[x.Id]
                : new MemberProperty
                {
                    ChapterPropertyId = x.Id,
                    MemberId = member.Id
                });

        return new MemberProfile(chapterId, member, allMemberProperties, chapterProperties);
    }

    public async Task<IReadOnlyCollection<Member>> GetMembers(Member? currentMember, Guid chapterId)
    {
        if (currentMember?.IsMemberOf(chapterId) != true)
        {
            return [];
        }

        var members = await _unitOfWork.MemberRepository.GetByChapterId(chapterId).RunAsync();
        return members
            .Where(x => x.Visible(chapterId))
            .ToArray();
    }

    public async Task<ServiceResult> PurchaseSubscription(Guid memberId, Guid chapterId, Guid chapterSubscriptionId,
        string cardToken)
    {
        var (member, chapterSubscription, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterSubscriptionRepository.GetByIdOrDefault(chapterSubscriptionId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId));
        if (chapterSubscription == null || chapterSubscription.ChapterId != chapterId)
        {
            return ServiceResult.Failure("Payment not made: subscription not found");
        }

        if (!member.IsMemberOf(chapterSubscription.ChapterId))
        {
            return ServiceResult.Failure("Payment not made: you are not a member of this subscription's chapter");
        }
        
        var paymentResult = await _paymentService.MakePayment(chapterSubscription.ChapterId, member, chapterSubscription.Amount, cardToken, 
            chapterSubscription.Title);
        if (!paymentResult.Success)
        {
            return ServiceResult.Failure($"Payment not made: {paymentResult.Message}");
        }

        var expiresUtc = (memberSubscription.ExpiresUtc ?? DateTime.UtcNow).AddMonths(chapterSubscription.Months);
        memberSubscription.ExpiresUtc = expiresUtc;
        memberSubscription.Type = chapterSubscription.Type;

        _unitOfWork.MemberSubscriptionRepository.Update(memberSubscription);

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

        var chapter = await _unitOfWork.ChapterRepository.GetById(chapterSubscription.ChapterId).RunAsync();
        var country = await _unitOfWork.CountryRepository.GetById(chapter.CountryId).RunAsync();
        
        await _emailService.SendEmail(chapter, member.GetEmailAddressee(), EmailType.SubscriptionConfirmation, new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
            { "subscription.amount", $"{country.CurrencySymbol}{chapterSubscription.Amount:0.00}" },
            { "subscription.end", chapter.ToLocalTime(expiresUtc).ToString("d MMMM yyyy") }
        });

        return ServiceResult.Successful();
    }        

    public async Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, Guid chapterId, string newEmailAddress)
    {
        var (chapter, member, existingToken) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberEmailAddressUpdateTokenRepository.GetByMemberId(memberId));

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
            MemberId = memberId,
            NewEmailAddress = newEmailAddress
        });

        var url = _chapterUrlService.GetChapterUrl(chapter, _settings.ConfirmEmailAddressUpdateUrlPath, new Dictionary<string, string>
        {
            { "token", HttpUtility.UrlEncode(activationToken) }
        });

        await _emailService.SendEmail(chapter, new EmailAddressee(newEmailAddress, member.FullName), EmailType.EmailAddressUpdate, 
            new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "url", url }
            });

        return ServiceResult.Successful();
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

        if (avatar == null)
        {
            avatar = new MemberAvatar();
        }

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

    public async Task SendActivationEmailAsync(Chapter? chapter, Member member, string activationToken)
    {
        var url = _chapterUrlService.GetChapterUrl(chapter, _settings.ActivateAccountUrlPath, new Dictionary<string, string>
        {
            { "token", HttpUtility.UrlEncode(activationToken) }
        });

        await _emailService.SendEmail(chapter, member.GetEmailAddressee(), EmailType.ActivateAccount, new Dictionary<string, string>
        {
            { "chapter.name", chapter?.Name ?? "" },
            { "url", url }
        });
    }

    public async Task UpdateMemberEmailOptIn(Guid memberId, bool optIn)
    {
        var member = await _unitOfWork.MemberRepository.GetById(memberId).RunAsync();
        if (member.EmailOptIn == optIn)
        {
            return;
        }

        member.EmailOptIn = optIn;

        _unitOfWork.MemberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateMemberImage(Guid id, UpdateMemberImage? model, MemberImageCropInfo cropInfo)
    {
        var (member, image, avatar) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(id),
            x => x.MemberImageRepository.GetByMemberId(id),
            x => x.MemberAvatarRepository.GetByMemberId(id));

        if (image == null)
        {
            image = new MemberImage();
        }

        if (avatar == null)
        {
            avatar = new MemberAvatar();
        }

        var result = _memberImageService.ProcessMemberImage(image, avatar, model, cropInfo);
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

    public async Task<ServiceResult> UpdateMemberChapterProfile(Guid id, Guid chapterId, UpdateMemberChapterProfile model)
    {
        var (chapterProperties, member, memberProperties) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(id),
            x => x.MemberPropertyRepository.GetByMemberId(id, chapterId));

        var validationResult = ValidateMemberProfile(chapterProperties, model);
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

        
        if (model.EmailOptIn != null)
        {
            member.EmailOptIn = model.EmailOptIn.Value;
        }

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
    
    public async Task<ServiceResult> UpdateMemberLocation(Guid id, LatLong? location, string? name)
    {
        var memberLocation = await _unitOfWork.MemberLocationRepository.GetByMemberId(id);

        if (memberLocation == null)
        {
            memberLocation = new MemberLocation();
        }

        if (location != null && !string.IsNullOrEmpty(name))
        {
            memberLocation.LatLong = location;
            memberLocation.Name = name;
        }
        else
        {
            memberLocation.LatLong = null;
            memberLocation.Name = null;
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

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberSiteProfile(Guid id, UpdateMemberSiteProfile model)
    {
        var member = await _unitOfWork.MemberRepository.GetById(id).RunAsync();

        member.FirstName = model.FirstName.Trim();
        member.LastName = model.LastName.Trim();

        _unitOfWork.MemberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private static IEnumerable<string> GetMissingMemberProfileProperties(CreateMemberProfile profile, IEnumerable<ChapterProperty> chapterProperties,
        IEnumerable<UpdateMemberProperty> memberProperties)
    {
        if (string.IsNullOrWhiteSpace(profile.EmailAddress))
        {
            yield return "Email address";
        }

        foreach (string property in GetMissingMemberProfileProperties(profile as UpdateMemberChapterProfile, chapterProperties, memberProperties))
        {
            yield return property;
        }
    }

    private static IEnumerable<string> GetMissingMemberProfileProperties(UpdateMemberChapterProfile profile, IEnumerable<ChapterProperty> chapterProperties,
        IEnumerable<UpdateMemberProperty> memberProperties)
    {
        var memberPropertyDictionary = memberProperties
            .ToDictionary(x => x.ChapterPropertyId, x => x.Value);
        foreach (var chapterProperty in chapterProperties.Where(x => x.Required))
        {
            memberPropertyDictionary.TryGetValue(chapterProperty.Id, out string? value);

            if (string.IsNullOrWhiteSpace(value))
            {
                yield return chapterProperty.Label;
            }
        }
    }        
    
    private ServiceResult ValidateMemberProfile(IReadOnlyCollection<ChapterProperty> chapterProperties, UpdateMemberChapterProfile profile)
    {
        IReadOnlyCollection<string> missingProperties = GetMissingMemberProfileProperties(profile, chapterProperties, profile.Properties).ToArray();

        if (missingProperties.Count > 0)
        {
            return ServiceResult.Failure($"The following properties are required: {string.Join(", ", missingProperties)}");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateMemberProfile(IReadOnlyCollection<ChapterProperty> chapterProperties, CreateMemberProfile profile)
    {
        IReadOnlyCollection<string> missingProperties = GetMissingMemberProfileProperties(profile, chapterProperties, profile.Properties).ToArray();

        if (missingProperties.Count > 0)
        {
            return ServiceResult.Failure($"The following properties are required: {string.Join(", ", missingProperties)}");
        }

        if (!MailUtils.ValidEmailAddress(profile.EmailAddress))
        {
            return ServiceResult.Failure("Invalid email address format");
        }

        return ServiceResult.Successful();
    }
}
