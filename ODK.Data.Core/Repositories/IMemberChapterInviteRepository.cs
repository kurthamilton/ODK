using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.Repositories;

public interface IMemberChapterInviteRepository : IWriteRepository<MemberChapterInvite>
{
    IDeferredQueryMultiple<MemberChapterInvite> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<MemberChapterInvite> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<MemberChapterInvite> GetByMemberId(Guid memberId, Guid chapterId);

    IDeferredQueryMultiple<MemberChapterInvite> GetByMemberIds(IReadOnlyCollection<Guid> memberIds);

    IDeferredQuerySingleOrDefault<MemberChapterInvite> GetByToken(string token);

    IDeferredQueryMultiple<MemberChapterInviteDto> GetDtosByChapterId(Guid chapterId);

    IDeferredQueryMultiple<MemberChapterInvite> GetCreatedBefore(DateTime createdBeforeUtc);
}
