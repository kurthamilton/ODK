using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberChapterRepository : ReadWriteRepositoryBase<MemberChapter, IMemberChapterQueryBuilder>, IMemberChapterRepository
{
    public MemberChapterRepository(DbContext context)
        : base(context)
    {
    }

    public override IMemberChapterQueryBuilder Query() =>
        CreateQueryBuilder(context => new MemberChapterQueryBuilder(context));
}