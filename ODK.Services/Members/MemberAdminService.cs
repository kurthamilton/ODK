using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Images;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Exceptions;
using ODK.Services.Imaging;

namespace ODK.Services.Members;

public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IImageService _imageService;
    private readonly IMemberService _memberService;
    private readonly MemberAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MemberAdminService(IUnitOfWork unitOfWork, IMemberService memberService, 
        ICacheService cacheService, IAuthorizationService authorizationService,
        IImageService imageService, MemberAdminServiceSettings settings)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _imageService = imageService;
        _memberService = memberService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task DeleteMember(Guid currentMemberId, Guid memberId)
    {
        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        AssertCurrentAdminMemberCanSeeMember(currentMemberId, chapterAdminMembers, member);

        _unitOfWork.MemberRepository.Delete(member);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Member> GetMember(Guid currentMemberId, Guid memberId)
    {
        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        AssertCurrentAdminMemberCanSeeMember(currentMemberId, chapterAdminMembers, member);

        return member;
    }
    
    public async Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(Guid currentMemberId, Guid chapterId)
    {
        var (members, subscriptions) = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.MemberRepository.GetByChapterId(chapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId));

        var csv = new List<IReadOnlyCollection<string>>
        {
            new []
            {
                "ID",
                "Email",
                "FirstName",
                "LastName",
                "Joined",
                "Activated",
                "Disabled",
                "EmailOptIn",
                "SubscriptionExpiryDate",
                "SubscriptionType"
            }
        };

        var subscriptionDictionary = subscriptions.ToDictionary(x => x.MemberId);

        foreach (var member in members.OrderBy(x => x.FullName))
        {
            subscriptionDictionary.TryGetValue(member.Id, out var subscription);

            csv.Add(
            [
                member.Id.ToString(),
                member.EmailAddress,
                member.FirstName,
                member.LastName,
                member.MemberChapter(chapterId).CreatedUtc.ToString("yyyy-MM-dd"),
                member.Activated ? "Y" : "",
                member.Disabled ? "Y" : "",
                member.EmailOptIn ? "Y" : "",
                subscription?.ExpiresUtc?.ToString("yyyy-MM-dd") ?? "",
                subscription?.Type.ToString() ?? ""
            ]);
        }

        return csv;
    }

    public async Task<IReadOnlyCollection<MemberAvatar>> GetMemberAvatars(Guid currentMemberId, Guid chapterId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.MemberAvatarRepository.GetByChapterId(chapterId));
    }

    public async Task<IReadOnlyCollection<MemberImage>> GetMemberImages(Guid currentMemberId, Guid chapterId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.MemberImageRepository.GetByChapterId(chapterId));
    }

    public async Task<MemberImage?> GetMemberImage(Guid currentMemberId, Guid chapterId, Guid memberId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
                    x => x.MemberImageRepository.GetByMemberId(memberId));
    }

    public Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId) => GetMembers(currentMemberId,
        new MemberFilter
        {
            ChapterId = chapterId,
            Statuses = Enum.GetValues<SubscriptionStatus>().ToList(),
            Types = Enum.GetValues<SubscriptionType>().ToList()
        });

    public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, MemberFilter filter)
    {
        var (members, memberSubscriptions, membershipSettings) = await GetChapterAdminRestrictedContent(currentMemberId, filter.ChapterId,
            x => x.MemberRepository.GetAllByChapterId(filter.ChapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(filter.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(filter.ChapterId));

        var memberSubscriptionsDictionary = memberSubscriptions.ToDictionary(x => x.MemberId);

        var filteredMembers = new List<Member>();
        foreach (var member in members)
        {
            if (!memberSubscriptionsDictionary.TryGetValue(member.Id, out var memberSubscription))
            {
                continue;
            }

            if (filter.Types.Contains(memberSubscription.Type))
            {
                filteredMembers.Add(member);
                continue;
            }

            if (membershipSettings == null)
            {
                continue;
            }

            var status = _authorizationService.GetSubscriptionStatus(memberSubscription, membershipSettings);
            if (filter.Statuses.Contains(status))
            {
                filteredMembers.Add(member);
                continue;
            }
        }

        return filteredMembers;
    }

    public async Task<MembersDto> GetMembersDto(Guid currentMemberId, Guid chapterId)
    {
        var (members, subscriptions) = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.MemberRepository.GetAllByChapterId(chapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId));

        return new MembersDto
        {
            Members = members,
            Subscriptions = subscriptions
        };
    }

    public async Task<MemberSubscription?> GetMemberSubscription(Guid currentMemberId, Guid chapterId, Guid memberId)
    {
        var (chapterAdminMembers, currentMember, member, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return memberSubscription;
    }

    public async Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId)
    {
        var (chapterAdminMembers, currentMember, subscriptions) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return subscriptions;
    }
    
    public async Task ResizeAllAvatars(Guid currentMemberId, Guid chapterId)
    {
        var avatars = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.MemberAvatarRepository.GetByChapterId(chapterId));

        foreach (var avatar in avatars)
        {
            avatar.ImageData = _imageService.Resize(avatar.ImageData, _settings.MemberAvatarSize, _settings.MemberAvatarSize);
            _unitOfWork.MemberAvatarRepository.Update(avatar);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RotateMemberImage(Guid currentMemberId, Guid memberId, int degrees)
    {
        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(memberId));
        
        await _memberService.RotateMemberImage(member.Id, degrees);

        _cacheService.RemoveVersionedItem<MemberImage>(memberId);
    }
    
    public async Task SendActivationEmail(Guid currentMemberId, Guid chapterId, Guid memberId)
    {
        var (chapter, currentMember, chapterAdminMembers, member, memberActivationToken) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberActivationTokenRepository.GetByMemberId(memberId));        

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        if (!member.IsMemberOf(chapterId) || memberActivationToken == null)
        {
            throw new OdkNotFoundException();
        }
        
        await _memberService.SendActivationEmailAsync(chapter, member, memberActivationToken.ActivationToken);
    }

    public async Task SetMemberVisibility(Guid currentMemberId, Guid memberId, Guid chapterId, bool visible)
    {
        var member = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.MemberRepository.GetById(memberId));

        if (!member.IsMemberOf(chapterId))
        {
            throw new OdkNotFoundException();
        }

        var privacySettings = member.PrivacySettings
            .FirstOrDefault(x => x.ChapterId == chapterId);

        if (privacySettings == null)
        {
            privacySettings = new MemberChapterPrivacySettings();
        }

        privacySettings.HideProfile = !visible;

        if (privacySettings.MemberId == Guid.Empty)
        {
            privacySettings.ChapterId = chapterId;
            privacySettings.MemberId = member.Id;
            _unitOfWork.MemberChapterPrivacySettingsRepository.Add(privacySettings);
        }
        else
        {
            _unitOfWork.MemberChapterPrivacySettingsRepository.Update(privacySettings);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateMemberAvatar(Guid currentMemberId, Guid chapterId, Guid memberId, MemberImageCropInfo cropInfo)
    {
        var (avatar, memberImage) = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.MemberAvatarRepository.GetByMemberId(memberId),
            x => x.MemberImageRepository.GetByMemberId(memberId));

        if (memberImage == null)
        {
            return;
        }

        if (avatar == null)
        {
            avatar = new MemberAvatar();
        }

        var imageData = memberImage.ImageData;
        if (cropInfo.CropWidth > 0 && cropInfo.CropHeight > 0 && cropInfo.CropX >= 0 && cropInfo.CropY >= 0)
        {
            imageData = _imageService.Crop(imageData, cropInfo.CropWidth, cropInfo.CropHeight, cropInfo.CropX, cropInfo.CropY);
        }

        imageData = _imageService.Resize(imageData, _settings.MemberAvatarSize, _settings.MemberAvatarSize);
        imageData = _imageService.Pad(imageData, _settings.MemberAvatarSize, _settings.MemberAvatarSize);

        var mimeType = _imageService.MimeType(imageData);
        if (mimeType == null)
        {
            throw new OdkServiceException("Error getting mime type");
        }

        avatar.ImageData = imageData;
        avatar.MimeType = mimeType;
        avatar.X = cropInfo.CropX;
        avatar.Y = cropInfo.CropY;
        avatar.Width = cropInfo.CropWidth;
        avatar.Height = cropInfo.CropHeight;

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
    }

    public async Task<ServiceResult> UpdateMemberSubscription(Guid currentMemberId, Guid chapterId, Guid memberId,
        UpdateMemberSubscription model)
    {
        var (chapterAdminMembers, currentMember, member, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        var expiryDate = model.Type == SubscriptionType.Alum 
            ? new DateTime?() 
            : model.ExpiryDate;

        if (memberSubscription == null)
        {
            memberSubscription = new MemberSubscription();
        }

        memberSubscription.ExpiresUtc = expiryDate;
        memberSubscription.Type = model.Type;

        var validationResult = ValidateMemberSubscription(memberSubscription);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (memberSubscription.MemberId == default)
        {
            memberSubscription.ChapterId = chapterId;
            memberSubscription.MemberId = memberId;
            _unitOfWork.MemberSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSubscriptionRepository.Update(memberSubscription);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private void AssertCurrentAdminMemberCanSeeMember(
        Guid currentMemberId,
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers,
        Member member)
    {
        if (!CurrentMemberCanSeeMember(currentMemberId, chapterAdminMembers, member))
        {
            throw new OdkNotAuthorizedException();
        }
    }

    private bool CurrentMemberCanSeeMember(
        Guid currentMemberId, 
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers, 
        Member member)
    {
        var chapterAdminMember = chapterAdminMembers
            .FirstOrDefault(x => x.MemberId == currentMemberId);
        return chapterAdminMember != null && member.SharesChapterWith(chapterAdminMember.Member);
    }

    private ServiceResult ValidateMemberSubscription(MemberSubscription subscription)
    {
        if (!Enum.IsDefined(typeof(SubscriptionType), subscription.Type) || subscription.Type == SubscriptionType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        if (subscription.Type == SubscriptionType.Alum && subscription.ExpiresUtc != null)
        {
            return ServiceResult.Failure("Alum should not have expiry date");
        }

        if (subscription.Type != SubscriptionType.Alum && subscription.ExpiresUtc < DateTime.UtcNow.Date)
        {
            return ServiceResult.Failure("Expiry date should not be in the past");
        }

        return ServiceResult.Successful();
    }
}
