using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterAdminMemberRepository : IWriteRepository<ChapterAdminMember>
{
    IDeferredQueryMultiple<ChapterAdminMember> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<ChapterAdminMember> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<ChapterAdminMember> GetByMemberId(Guid memberId, Guid chapterId);
}
