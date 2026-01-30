using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterAdminMemberRepository : WriteRepositoryBase<ChapterAdminMember>, IChapterAdminMemberRepository
{
    private readonly PlatformType _platform;

    public ChapterAdminMemberRepository(OdkContext context, IPlatformProvider platformProvider)
        : base(context)
    {
        _platform = platformProvider.GetPlatform();
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

    protected override IQueryable<ChapterAdminMember> Set()
    {
        var chapterQuery =
            from chapter in Set<Chapter>()
            select chapter;

        if (_platform != PlatformType.Default)
        {
            chapterQuery = chapterQuery
                .Where(x => x.Platform == _platform);
        }

        var query =
            from chapterAdminMember in base.Set()
                .Include(x => x.Member)
                .ThenInclude(x => x.Chapters)
                .Include(x => x.Member)
            from chapter in chapterQuery
                .Where(x => x.Id == chapterAdminMember.ChapterId)
            select chapterAdminMember;

        return query;
    }
}