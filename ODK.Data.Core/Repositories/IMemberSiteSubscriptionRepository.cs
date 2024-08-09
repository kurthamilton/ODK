using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberSiteSubscriptionRepository : IWriteRepository<MemberSiteSubscription>
{
    IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByMemberId(Guid memberId);
}
