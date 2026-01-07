using ODK.Core.Members;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberChapterRepository : WriteRepositoryBase<MemberChapter>, IMemberChapterRepository
{
    public MemberChapterRepository(OdkContext context)
        : base(context)
    {
    }
}
