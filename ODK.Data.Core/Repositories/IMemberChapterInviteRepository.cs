using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberChapterInviteRepository : IWriteRepository<MemberChapterInvite>
{
    /// <summary>
    /// Every outstanding invitation for the chapter. The import reads these as a set so it can skip anyone it
    /// has already invited - the rows it needs are keyed by member, but it only knows email addresses until the
    /// members are resolved, and one query alongside the others beats one per row.
    /// </summary>
    IDeferredQueryMultiple<MemberChapterInvite> GetByChapterId(Guid chapterId);

    /// <summary>
    /// Every outstanding invitation for a member, across chapters. Read before an unactivated account is
    /// discarded and recreated, so the invitations can be carried over rather than cascaded away.
    /// </summary>
    IDeferredQueryMultiple<MemberChapterInvite> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<MemberChapterInvite> GetByMemberId(Guid memberId, Guid chapterId);

    /// <summary>
    /// The invitation an emailed link identifies, for a member who cannot sign in yet.
    /// </summary>
    IDeferredQuerySingleOrDefault<MemberChapterInvite> GetByToken(string token);
}
