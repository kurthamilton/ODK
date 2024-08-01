using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Emails;
using ODK.Core.Images;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Services.Imaging;
using ODK.Services.Payments;

namespace ODK.Services.Members;

public class MemberService : IMemberService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly IImageService _imageService;
    private readonly IPaymentService _paymentService;
    private readonly MemberServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(IUnitOfWork unitOfWork, IAuthorizationService authorizationService,
        IEmailService emailService, MemberServiceSettings settings, IImageService imageService, IPaymentService paymentService,
        ICacheService cacheService)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _emailService = emailService;
        _imageService = imageService;
        _paymentService = paymentService;
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
    
    public async Task<ServiceResult> CreateMember(Guid chapterId, CreateMemberProfile model)
    {
        var (chapter, chapterProperties, membershipSettings, existing, siteSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetByEmailAddress(model.EmailAddress),
            x => x.SiteSettingsRepository.Get());

        var validationResult = ValidateMemberProfile(chapterProperties, model);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        var imageResult = ValidateMemberImage(model.Image.MimeType, model.Image.ImageData);
        if (!imageResult.Success)
        {
            return imageResult;
        }

        if (existing != null)
        {
            return ServiceResult.Failure("Email address already in use");
        }

        var member = new Member
        {
            Activated = false,
            CreatedUtc = DateTime.UtcNow,
            Disabled = false,
            EmailAddress = model.EmailAddress,
            EmailOptIn = model.EmailOptIn ?? false,
            FirstName = model.FirstName,
            LastName = model.LastName,
            SuperAdmin = false            
        };
        _unitOfWork.MemberRepository.Add(member);

        member.Chapters.Add(new MemberChapter
        {
            MemberId = member.Id,
            ChapterId = chapterId
        });

        var subscription = new MemberSubscription
        {
            ChapterId = chapterId,
            ExpiresUtc = member.CreatedUtc
                .AddMonths(membershipSettings?.TrialPeriodMonths ?? siteSettings.DefaultTrialPeriodMonths),
            MemberId = member.Id,
            Type = SubscriptionType.Trial
        };
        _unitOfWork.MemberSubscriptionRepository.Add(subscription);

        var image = new MemberImage
        {
            ImageData = model.Image.ImageData,
            MemberId = member.Id,
            MimeType = model.Image.MimeType
        };
        PrepareMemberImage(image);
        _unitOfWork.MemberImageRepository.Add(image);

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

    public async Task<IReadOnlyCollection<Member>> GetLatestMembers(Member currentMember, Guid chapterId)
    {
        await _authorizationService.AssertMemberIsChapterMemberAsync(currentMember.Id, chapterId);

        var members = await GetMembers(currentMember, chapterId);

        return members
            .OrderByDescending(x => x.CreatedUtc)
            .Take(8)
            .ToArray();
    }

    public async Task<Member?> GetMember(Guid memberId, Guid chapterId)
    {
        var member = await _unitOfWork.MemberRepository.GetByIdOrDefault(memberId).RunAsync();
        return member?.IsMemberOf(chapterId) == true
            ? member 
            : null;
    }

    public async Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? size)
    {
        return await GetMemberImage(currentVersion, memberId, size, size);
    }

    public async Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId,
        int? width, int? height)
    {
        var result = await _cacheService.GetOrSetVersionedItem(
            () => _unitOfWork.MemberImageRepository.GetByMemberId(memberId).RunAsync(),
            memberId,
            currentVersion);

        if (currentVersion == result.Version)
        {
            return result;
        }

        MemberImage? image = result.Value;
        if (image == null)
        {
            return new VersionedServiceResult<MemberImage>(0, null);
        }

        if (width > 0 || height > 0)
        {
            int w = NumberUtils.FirstPositive(width, height) ?? 0;
            int h = NumberUtils.FirstPositive(height, width) ?? 0;

            byte[] imageData = _imageService.Crop(image.ImageData, w, h);
            image = new MemberImage
            {
                ImageData = imageData,
                MemberId = memberId,
                MimeType = image.MimeType,
                Version = image.Version
            };
        }

        return new VersionedServiceResult<MemberImage>(BitConverter.ToInt64(image.Version), image);
    }

    public async Task<MemberProfile?> GetMemberProfile(Guid chapterId, Member currentMember, Member? member)
    {
        if (member == null || !member.CanBeViewedBy(currentMember))
        {
            return null;
        }

        return await GetMemberProfile(member, chapterId);
    }

    public async Task<IReadOnlyCollection<Member>> GetMembers(Member? currentMember, Guid chapterId)
    {
        if (currentMember?.IsMemberOf(chapterId) != true)
        {
            return [];
        }

        return await _unitOfWork.MemberRepository.GetByChapterId(chapterId).RunAsync();
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
            { "subscription.end", chapter.ToChapterTime(expiresUtc).ToString("d MMMM yyyy") }
        });

        return ServiceResult.Successful();
    }
    
    public async Task RotateMemberImage(Guid memberId, int degrees)
    {
        var (member, image) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberImageRepository.GetByMemberId(memberId));
        
        if (image == null)
        {
            return;
        }

        var data = _imageService.Rotate(image.ImageData, degrees);
        image.ImageData = data;

        _unitOfWork.MemberImageRepository.Update(image);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<MemberImage>(memberId);
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

        string activationToken = RandomStringGenerator.Generate(64);

        var token = new MemberEmailAddressUpdateToken
        {
            ConfirmationToken = activationToken,
            MemberId = memberId,
            NewEmailAddress = newEmailAddress
        };

        _unitOfWork.MemberEmailAddressUpdateTokenRepository.Add(token);

        var url = _settings.ConfirmEmailAddressUpdateUrl.Interpolate(new Dictionary<string, string>
        {
            { "chapter.name", chapter!.Name },
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

    public async Task SendActivationEmailAsync(Chapter chapter, Member member, string activationToken)
    {
        var url = _settings.ActivateAccountUrl.Interpolate(new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
            { "token", HttpUtility.UrlEncode(activationToken) }
        });

        await _emailService.SendEmail(chapter, member.GetEmailAddressee(), EmailType.ActivateAccount, new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
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

    public async Task<ServiceResult> UpdateMemberImage(Guid id, UpdateMemberImage model)
    {
        var (member, image) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(id),
            x => x.MemberImageRepository.GetByMemberId(id));

        image.MimeType = model.MimeType;
        image.ImageData = model.ImageData;

        var validationResult = ValidateMemberImage(image.MimeType, image.ImageData);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        PrepareMemberImage(image);        

        _unitOfWork.MemberImageRepository.Update(image);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<MemberImage>(id);

        return ServiceResult.Successful("Picture updated");
    }

    public async Task<ServiceResult> UpdateMemberProfile(Guid id, Guid chapterId, UpdateMemberProfile model)
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

        member.FirstName = model.FirstName.Trim();
        member.LastName = model.LastName.Trim();

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
    
    private static IEnumerable<string> GetMissingMemberProfileProperties(CreateMemberProfile profile, IEnumerable<ChapterProperty> chapterProperties,
        IEnumerable<UpdateMemberProperty> memberProperties)
    {
        if (string.IsNullOrWhiteSpace(profile.EmailAddress))
        {
            yield return "Email address";
        }

        foreach (string property in GetMissingMemberProfileProperties(profile as UpdateMemberProfile, chapterProperties, memberProperties))
        {
            yield return property;
        }
    }

    private static IEnumerable<string> GetMissingMemberProfileProperties(UpdateMemberProfile profile, IEnumerable<ChapterProperty> chapterProperties,
        IEnumerable<UpdateMemberProperty> memberProperties)
    {
        if (string.IsNullOrWhiteSpace(profile.FirstName))
        {
            yield return "First name";
        }

        if (string.IsNullOrWhiteSpace(profile.LastName))
        {
            yield return "First name";
        }

        IDictionary<Guid, string> memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId, x => x.Value);
        foreach (ChapterProperty chapterProperty in chapterProperties.Where(x => x.Required))
        {
            memberPropertyDictionary.TryGetValue(chapterProperty.Id, out string? value);

            if (string.IsNullOrWhiteSpace(value))
            {
                yield return chapterProperty.Label;
            }
        }
    }        

    private byte[] EnforceMaxImageSize(byte[] imageData)
    {
        return _imageService.Reduce(imageData, _settings.MaxImageSize, _settings.MaxImageSize);
    }

    private async Task<MemberProfile> GetMemberProfile(Member member, Guid chapterId)
    {
        var (chapterProperties, memberProperties) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.MemberPropertyRepository.GetByMemberId(member.Id, chapterId));

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

        return new MemberProfile(member, allMemberProperties, chapterProperties);
    }
    
    private void PrepareMemberImage(MemberImage image)
    {
        var data = EnforceMaxImageSize(image.ImageData);
        image.ImageData = data;
    }

    private ServiceResult ValidateMemberImage(string mimeType, byte[] data)
    {
        if (!ImageValidator.IsValidMimeType(mimeType) || 
            !_imageService.IsImage(data))
        {
            return ServiceResult.Failure("File is not a valid image");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateMemberProfile(IReadOnlyCollection<ChapterProperty> chapterProperties, UpdateMemberProfile profile)
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
