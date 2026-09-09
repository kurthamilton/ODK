using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberChapterImportRepository
    : ReadWriteRepositoryBase<MemberChapterImport, IMemberChapterImportQueryBuilder>, IMemberChapterImportRepository
{
    public MemberChapterImportRepository(DbContext context)
        : base(context)
    {
    }

    public override IMemberChapterImportQueryBuilder Query()
        => CreateQueryBuilder(context => new MemberChapterImportQueryBuilder(context));
}
