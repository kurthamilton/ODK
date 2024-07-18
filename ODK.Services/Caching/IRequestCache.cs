using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Chapters;

namespace ODK.Services.Caching;

public interface IRequestCache
{
    Task<Chapter> GetChapterAsync(Guid chapterId);

    Task<Chapter> GetChapterAsync(string name);    

    Task<ChapterMembershipSettings?> GetChapterMembershipSettingsAsync(Guid chapterId);

    Task<IReadOnlyCollection<Chapter>> GetChaptersAsync();

    Task<ChaptersDto> GetChaptersDtoAsync();

    Task<Member?> GetMemberAsync(Guid memberId);

    Task<MemberSubscription?> GetMemberSubscriptionAsync(Guid memberId);
}
