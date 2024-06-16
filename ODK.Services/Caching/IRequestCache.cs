using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Caching;

public interface IRequestCache
{
    Task<Chapter?> GetChapterAsync(Guid chapterId);

    Task<Chapter?> GetChapterAsync(string name);

    Task<IReadOnlyCollection<Chapter>> GetChaptersAsync();

    Task<ChapterMembershipSettings?> GetChapterMembershipSettingsAsync(Guid chapterId);

    Task<Member?> GetMemberAsync(Guid memberId);

    Task<MemberSubscription?> GetMemberSubscriptionAsync(Guid memberId);
}
