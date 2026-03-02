using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberChapterQueryBuilder
    : DatabaseEntityQueryBuilder<MemberChapter, IMemberChapterQueryBuilder>, IMemberChapterQueryBuilder
{
    public MemberChapterQueryBuilder(DbContext context, PlatformType platform)
        : base(context, BaseQuery(context, platform))
    {
    }

    protected override IMemberChapterQueryBuilder Builder => this;

    public IMemberChapterQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public IQueryBuilder<MemberChapterDto> ToDto()
    {
        var query =
            from memberChapter in Query
            from chapter in Set<Chapter>()
                .Where(x => x.Id == memberChapter.ChapterId)
            select new MemberChapterDto
            {
                Chapter = chapter,
                MemberChapter = memberChapter
            };

        return ProjectTo(query);
    }

    private static IQueryable<MemberChapter> BaseQuery(DbContext context, PlatformType platform)
    {
        return
            from memberChapter in context.Set<MemberChapter>()
            from chapter in context.Set<Chapter>()
                .ForPlatform(platform, includeUnpublished: true)
                .Where(x => x.Id == memberChapter.ChapterId)
            select memberChapter;
    }
}