using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services.Caching;

public interface IRequestCache
{    
    Task<Chapter> GetChapterAsync(PlatformType platform, Guid chapterId);

    Task<Chapter> GetChapterAsync(PlatformType platform, string name);

    Task<IReadOnlyCollection<Chapter>> GetChaptersAsync(PlatformType platform);

    Task<ChapterMembershipSettings?> GetChapterMembershipSettingsAsync(Guid chapterId);

    Task<Member?> GetMemberAsync(Guid memberId);

    Task<MemberSubscription?> GetMemberSubscriptionAsync(Guid memberId, Guid chapterId);
}
