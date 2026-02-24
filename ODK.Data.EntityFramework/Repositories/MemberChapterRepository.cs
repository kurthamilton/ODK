using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberChapterRepository : WriteRepositoryBase<MemberChapter>, IMemberChapterRepository
{
    public MemberChapterRepository(DbContext context)
        : base(context)
    {
    }
}