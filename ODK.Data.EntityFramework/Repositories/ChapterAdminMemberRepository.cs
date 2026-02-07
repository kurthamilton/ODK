using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterAdminMemberRepository : WriteRepositoryBase<ChapterAdminMember>, IChapterAdminMemberRepository
{
    public ChapterAdminMemberRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterAdminMember> GetByChapterId(PlatformType platform, Guid chapterId)
        => Set(platform)
            .Where(x => x.ChapterId == chapterId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterAdminMember> GetByMemberId(PlatformType platform, Guid memberId)
        => Set(platform)
            .Where(x => x.MemberId == memberId)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<ChapterAdminMember> GetByMemberId(PlatformType platform, Guid memberId, Guid chapterId)
        => Set(platform)
            .Where(x => x.MemberId == memberId && x.ChapterId == chapterId)
            .DeferredSingleOrDefault();

    public IDeferredQuery<bool> IsAdmin(PlatformType platform, Guid chapterId, Guid memberId)
    {
        var query =
            from member in Set<Member>()
            from adminMember in Set(platform)
                .Where(x => x.MemberId == member.Id && x.ChapterId == chapterId)
                .DefaultIfEmpty()
            where
                member.Id == memberId &&
                (member.SiteAdmin || adminMember.MemberId == member.Id)
            select member.Id;

        return query.DeferredAny();
    }

    public IDeferredQuery<bool> IsAdmin(PlatformType platform, Guid memberId)
        => Set(platform)
            .Where(x => x.MemberId == memberId)
            .DeferredAny();

    protected IQueryable<ChapterAdminMember> Set(PlatformType platform)
    {
        var chapterQuery =
            from chapter in Set<Chapter>()
                .ForPlatform(platform, includeUnpublished: true)
            select chapter;

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