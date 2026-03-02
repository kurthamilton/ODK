using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberChapterRepository : ReadWriteRepositoryBase<MemberChapter>, IMemberChapterRepository
{
    public MemberChapterRepository(DbContext context)
        : base(context)
    {
    }

    public IMemberChapterQueryBuilder Query(PlatformType platform) =>
        CreateQueryBuilder(context => new MemberChapterQueryBuilder(context, platform));
}