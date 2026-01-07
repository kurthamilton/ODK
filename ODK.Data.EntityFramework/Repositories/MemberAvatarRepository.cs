using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberAvatarRepository : WriteRepositoryBase<MemberAvatar>, IMemberAvatarRepository
{
    public MemberAvatarRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberAvatar> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredSingleOrDefault();
}
