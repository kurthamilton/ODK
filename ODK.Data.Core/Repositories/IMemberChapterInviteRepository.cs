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

    IDeferredQuerySingleOrDefault<MemberChapterInvite> GetByToken(string? token);

    /// <summary>How many invites the group has raised, sent or not, for a page that reports the number.</summary>
    IDeferredQuery<int> GetCountByChapterId(Guid chapterId);

    IDeferredQueryMultiple<MemberChapterInviteDto> GetDtosByChapterId(Guid chapterId);

    IDeferredQueryMultiple<MemberChapterInvite> GetCreatedBefore(DateTime createdBeforeUtc);

    /// <summary>The group's invites whose email has yet to be sent - see <see cref="MemberChapterInvite.SentUtc"/>.</summary>
    IDeferredQueryMultiple<MemberChapterInvite> GetUnsentByChapterId(Guid chapterId);

    /// <summary>How many invites the group is holding, for a page that reports the number rather than them.</summary>
    IDeferredQuery<int> GetUnsentCountByChapterId(Guid chapterId);
}
