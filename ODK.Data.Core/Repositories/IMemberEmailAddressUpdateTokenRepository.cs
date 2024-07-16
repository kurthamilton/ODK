using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IMemberEmailAddressUpdateTokenRepository : IWriteRepository<MemberEmailAddressUpdateToken>
{
    IDeferredQuerySingleOrDefault<MemberEmailAddressUpdateToken> GetByMemberId(Guid memberId);
}
