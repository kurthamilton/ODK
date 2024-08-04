using System.Diagnostics.CodeAnalysis;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Caching;

namespace ODK.Services.Authorization;

public class AuthorizationService : IAuthorizationService
{
    private readonly IRequestCache _requestCache;
    private readonly IUnitOfWork _unitOfWork;

    public AuthorizationService(IUnitOfWork unitOfWork,
        IRequestCache requestCache)
    {        
        _requestCache = requestCache;
        _unitOfWork = unitOfWork;
    }

    public void AssertMemberIsCurrent([NotNull] Member? member)
        => OdkAssertions.MeetsCondition(member, x => x.IsCurrent());

    public SubscriptionStatus GetSubscriptionStatus(MemberSubscription subscription, 
        ChapterMembershipSettings membershipSettings)
    {
        if (subscription.Type == SubscriptionType.Alum)
        {
            return SubscriptionStatus.Disabled;
        }

        if (subscription.ExpiresUtc == null || 
            !membershipSettings.Enabled ||
            membershipSettings.MembershipDisabledAfterDaysExpired <= 0)
        {
            return SubscriptionStatus.Current;
        }

        if (subscription.ExpiresUtc >= DateTime.UtcNow)
        {
            return subscription.ExpiresUtc >= DateTime.UtcNow.AddDays(membershipSettings.MembershipExpiringWarningDays)
                ? SubscriptionStatus.Current
                : SubscriptionStatus.Expiring;
        }

        return subscription.ExpiresUtc >= DateTime.UtcNow.AddDays(-1 * membershipSettings.MembershipDisabledAfterDaysExpired)
            ? SubscriptionStatus.Expired
            : SubscriptionStatus.Disabled;
    }

    public async Task<bool> MembershipIsActiveAsync(Guid memberId, Guid chapterId)
    {
        var subscription = await _requestCache.GetMemberSubscriptionAsync(memberId, chapterId);
        if (subscription == null)
        {
            return false;
        }
        
        return await MembershipIsActiveAsync(subscription, chapterId);
    }

    public async Task<bool> MembershipIsActiveAsync(MemberSubscription subscription, Guid chapterId)
    {
        var membershipSettings = await _unitOfWork.ChapterMembershipSettingsRepository
            .GetByChapterId(chapterId)
            .RunAsync();

        return MembershipIsActive(subscription, membershipSettings);
    }

    public bool MembershipIsActive(MemberSubscription subscription, ChapterMembershipSettings? membershipSettings)
    {
        if (subscription.Type == SubscriptionType.Alum)
        {
            return false;
        }

        if (subscription.ExpiresUtc == null || subscription.ExpiresUtc >= DateTime.UtcNow)
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

        return subscription.ExpiresUtc >= DateTime.UtcNow.AddDays(-1 * membershipSettings.MembershipDisabledAfterDaysExpired);
    }

    private async Task<Member> GetMemberAsync(Guid id)
    {
        var member = await _unitOfWork.MemberRepository.GetByIdOrDefault(id).RunAsync();
        AssertMemberIsCurrent(member);
        return member;
    }

    private async Task<MemberSubscription?> GetMemberSubscriptionAsync(Guid memberId, Guid chapterId)
    {
        return await _unitOfWork.MemberSubscriptionRepository
            .GetByMemberId(memberId, chapterId)
            .RunAsync();
    }
}
