using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IMemberRepository : IReadWriteRepository<Member, IMemberQueryBuilder>
{
    IDeferredQueryMultiple<Member> GetAllByChapterId(Guid chapterId);

    IDeferredQueryMultiple<MemberWithAvatarDto> GetAllWithAvatarByChapterId(Guid chapterId);

    IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<MemberWithAvatarDto> GetByChapterId(Guid chapterId, IEnumerable<Guid> memberIds);

    IDeferredQueryMultiple<Member> GetByChapterIds(IEnumerable<Guid> chapterIds);

    IDeferredQuerySingleOrDefault<Member> GetByEmailAddress(string emailAddress);

    IDeferredQuerySingle<Member> GetChapterOwner(Guid chapterId);

    IDeferredQuery<int> GetCountByChapterId(Guid chapterId);

    /// <summary>
    /// The members who most recently joined a chapter, newest first. Ordered by the date they joined the
    /// chapter, unlike <see cref="GetLatestWithAvatarByChapterId"/>, which orders by when they signed up.
    /// </summary>
    IDeferredQueryMultiple<MemberChapterWithAvatarDto> GetLatestJoinedByChapterId(Guid chapterId, int pageSize);

    IDeferredQueryMultiple<MemberWithAvatarDto> GetLatestWithAvatarByChapterId(Guid chapterId, int pageSize);

    IDeferredQuerySingle<MemberWithAvatarDto> GetWithAvatarById(Guid memberId);
}