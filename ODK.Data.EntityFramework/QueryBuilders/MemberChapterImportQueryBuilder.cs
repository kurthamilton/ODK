using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberChapterImportQueryBuilder
    : DatabaseEntityQueryBuilder<MemberChapterImport, IMemberChapterImportQueryBuilder>, IMemberChapterImportQueryBuilder
{
    public MemberChapterImportQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IMemberChapterImportQueryBuilder Builder => this;

    public IMemberChapterImportQueryBuilder CreatedBefore(DateTime createdBeforeUtc)
    {
        Query = Query.Where(x => x.CreatedUtc < createdBeforeUtc);
        return this;
    }

    public IMemberChapterImportQueryBuilder InChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }
}
