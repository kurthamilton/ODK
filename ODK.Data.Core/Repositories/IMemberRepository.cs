using ODK.Core.Members;
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

    IDeferredQuerySingleOrDefault<Member> GetByEmailAddress(string emailAddress);

    IDeferredQuerySingle<Member> GetChapterOwner(Guid chapterId);

    IDeferredQuery<int> GetCountByChapterId(Guid chapterId);

    IDeferredQueryMultiple<MemberWithAvatarDto> GetLatestWithAvatarByChapterId(Guid chapterId, int pageSize);

    IDeferredQuerySingle<MemberWithAvatarDto> GetWithAvatarById(Guid memberId);
}