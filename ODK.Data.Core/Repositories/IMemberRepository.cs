using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberRepository : IReadWriteRepository<Member>
{
    IDeferredQueryMultiple<Member> GetAllByChapterId(Guid chapterId);    
    IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId);
    IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId, IEnumerable<Guid> memberIds);
    IDeferredQuerySingleOrDefault<Member> GetByEmailAddress(string emailAddress);
    IDeferredQueryMultiple<Member> GetByEmailAddresses(IEnumerable<string> emailAddresses);
    IDeferredQuery<int> GetCountByChapterId(Guid chapterId);
    IDeferredQueryMultiple<Member> GetLatestByChapterId(Guid chapterId, int pageSize);
}