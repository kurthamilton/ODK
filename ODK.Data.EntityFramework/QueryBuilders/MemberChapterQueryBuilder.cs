using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberChapterQueryBuilder
    : DatabaseEntityQueryBuilder<MemberChapter, IMemberChapterQueryBuilder>, IMemberChapterQueryBuilder
{
    public MemberChapterQueryBuilder(DbContext context)
        : base(context)
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
}