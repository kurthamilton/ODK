using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IChapterAdminMemberRepository : IWriteRepository<ChapterAdminMember>
{
    IDeferredQueryMultiple<ChapterAdminMember> GetByChapterId(PlatformType platform, Guid chapterId);

    IDeferredQueryMultiple<ChapterAdminMember> GetByMemberId(PlatformType platform, Guid memberId);

    IDeferredQuerySingleOrDefault<ChapterAdminMember> GetByMemberId(PlatformType platform, Guid memberId, Guid chapterId);

    IDeferredQueryMultiple<ChapterAdminMemberWithAvatarDto> GetAdminMembersWithAvatarsByChapterId(
        PlatformType platform, Guid chapterId);

    IDeferredQuery<bool> IsAdmin(PlatformType platform, Guid chapterId, Guid memberId);

    IChapterAdminMemberQueryBuilder Query(PlatformType platform);
}