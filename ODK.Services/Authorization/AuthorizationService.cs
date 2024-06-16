using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Authorization;

public class AuthorizationService : IAuthorizationService
{
    private readonly IChapterRepository _chapterRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IRequestCache _requestCache;

    public AuthorizationService(IMemberRepository memberRepository,
        IChapterRepository chapterRepository,
        IRequestCache requestCache)
    {
        _chapterRepository = chapterRepository;
        _memberRepository = memberRepository;
        _requestCache = requestCache;
    }

    public async Task AssertMemberIsChapterMemberAsync(Guid memberId, Guid chapterId)
    {
        Member member = await GetMemberAsync(memberId);
        await AssertMemberIsChapterMemberAsync(member, chapterId);
    }

    public async Task AssertMemberIsChapterMemberAsync(Member member, Guid chapterId)
    {
        AssertMemberIsCurrent(member);
        if (member.ChapterId == chapterId)
        {
            return;
        }

        ChapterAdminMember? chapterAdminMember = await _chapterRepository.GetChapterAdminMember(chapterId, member.Id);
        if (chapterAdminMember != null)
        {
            return;
        }

        throw new OdkNotAuthorizedException();
    }

    public async Task AssertMemberIsCurrent(Guid memberId)
    {
        Member member = await GetMemberAsync(memberId);
        AssertMemberIsCurrent(member);
    }

    public void AssertMemberIsCurrent(Member? member)
    {
        if (member == null || !member.IsCurrent())
        {
            throw new OdkNotFoundException();
        }
    }

    public async Task AssertMembershipIsActiveAsync(Guid memberId, Guid chapterId)
    {
        MemberSubscription? subscription = await GetMemberSubscriptionAsync(memberId);
        if (subscription == null || !await MembershipIsActiveAsync(subscription, chapterId))
        {
            throw new OdkNotAuthorizedException();
        }
    }

    public string? GetRestrictedContentMessage(Member? member, Chapter? chapter, MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings)
    {
        if (chapter == null)
        {
            return "Chapter not found";
        }

        var defaultMessage = $"This page is only visible to {chapter.Name} members";
        if (member == null ||
            member.ChapterId != chapter.Id ||
            subscription == null ||
            membershipSettings == null)
        {
            return defaultMessage;
        }

        var subscriptionStatus = GetSubscriptionStatus(subscription, membershipSettings);
        switch (subscriptionStatus)
        {
            case SubscriptionStatus.Disabled:
                return defaultMessage;
            default:
                return null;
        }
    }

    public async Task<string?> GetRestrictedContentMessageAsync(Guid? memberId, Chapter? chapter)
    {
        var member = memberId != null
            ? await _requestCache.GetMemberAsync(memberId.Value)
            : null;
        var memberSubscription = memberId != null
            ? await _requestCache.GetMemberSubscriptionAsync(memberId.Value)
            : null;
        var membershipSettings = chapter != null
            ? await _requestCache.GetChapterMembershipSettingsAsync(chapter.Id)
            : null;

        return GetRestrictedContentMessage(member, chapter, memberSubscription, membershipSettings);
    }

    public SubscriptionStatus GetSubscriptionStatus(MemberSubscription subscription, 
        ChapterMembershipSettings membershipSettings)
    {
        if (subscription.Type == SubscriptionType.Alum)
        {
            return SubscriptionStatus.Disabled;
        }

        if (subscription.ExpiryDate == null || 
            !membershipSettings.Enabled ||
            membershipSettings.MembershipDisabledAfterDaysExpired <= 0)
        {
            return SubscriptionStatus.Current;
        }

        if (subscription.ExpiryDate >= DateTime.UtcNow)
        {
            return subscription.ExpiryDate >= DateTime.UtcNow.AddDays(membershipSettings.MembershipExpiringWarningDays)
                ? SubscriptionStatus.Current
                : SubscriptionStatus.Expiring;
        }

        return subscription.ExpiryDate >= DateTime.UtcNow.AddDays(-1 * membershipSettings.MembershipDisabledAfterDaysExpired)
            ? SubscriptionStatus.Expired
            : SubscriptionStatus.Disabled;
    }

    public async Task<bool> MembershipIsActiveAsync(Guid memberId, Guid chapterId)
    {
        var subscription = await _requestCache.GetMemberSubscriptionAsync(memberId);
        if (subscription == null)
        {
            return false;
        }
        
        return await MembershipIsActiveAsync(subscription, chapterId);
    }

    public async Task<bool> MembershipIsActiveAsync(MemberSubscription subscription, Guid chapterId)
    {
        ChapterMembershipSettings? membershipSettings = await _chapterRepository.GetChapterMembershipSettings(chapterId);

        return MembershipIsActive(subscription, membershipSettings);
    }

    public bool MembershipIsActive(MemberSubscription subscription, ChapterMembershipSettings? membershipSettings)
    {
        if (subscription.Type == SubscriptionType.Alum)
        {
            return false;
        }

        if (subscription.ExpiryDate == null || subscription.ExpiryDate >= DateTime.UtcNow)
        {
            return true;
        }

        if (membershipSettings == null)
        {
            return false;
        }

        if (!membershipSettings.Enabled || membershipSettings.MembershipDisabledAfterDaysExpired <= 0)
        {
            return true;
        }

        return subscription.ExpiryDate >= DateTime.UtcNow.AddDays(-1 * membershipSettings.MembershipDisabledAfterDaysExpired);
    }

    private async Task<Member> GetMemberAsync(Guid id)
    {
        Member? member = await _memberRepository.GetMemberAsync(id, true);
        AssertMemberIsCurrent(member);
        return member!;
    }

    private async Task<MemberSubscription?> GetMemberSubscriptionAsync(Guid memberId)
    {
        return await _memberRepository.GetMemberSubscriptionAsync(memberId);
    }
}
