using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterAdminMemberRepository : WriteRepositoryBase<ChapterAdminMember>, IChapterAdminMemberRepository
{
    public ChapterAdminMemberRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterAdminMember> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterAdminMember> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();

    public IDeferredQuerySingle<ChapterAdminMember> GetByMemberId(Guid memberId, Guid chapterId) => Set()
        .Where(x => x.MemberId == memberId && x.ChapterId == chapterId)
        .DeferredSingle();

    protected override IQueryable<ChapterAdminMember> Set() => base.Set()
        .Include(x => x.Member)
        .ThenInclude(x => x.Chapters)
        .Include(x => x.Member)
        .ThenInclude(x => x.PrivacySettings);
}
