using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IMemberAvatarRepository : IWriteRepository<MemberAvatar>
{
    IDeferredQueryMultiple<MemberAvatar> GetByChapterId(Guid chapterId);
    IDeferredQuerySingleOrDefault<MemberAvatar> GetByMemberId(Guid memberId);
}
