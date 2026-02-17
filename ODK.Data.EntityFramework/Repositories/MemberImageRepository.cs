using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberImageRepository : WriteRepositoryBase<MemberImage>, IMemberImageRepository
{
    public MemberImageRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberImage> GetByMemberId(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<MemberImageVersionDto> GetVersionDtoByMemberId(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .Select(x => new MemberImageVersionDto
            {
                MemberId = x.MemberId,
                Version = x.VersionInt
            })
            .DeferredSingleOrDefault();
}