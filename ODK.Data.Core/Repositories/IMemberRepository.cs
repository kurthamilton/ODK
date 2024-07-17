using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberRepository : IReadWriteRepository<Member>
{
    IDeferredQueryMultiple<Member> GetAdminMembersByChapterId(Guid chapterId);
    IDeferredQuerySingleOrDefault<Member> GetByEmailAddress(string emailAddress);
    IDeferredQuerySingleOrDefault<Member> GetByIdOrDefault(Guid memberId, bool searchAll);
    IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId, bool searchAll = false);
}