using ODK.Core.Members;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberQueryBuilder : IDatabaseEntityQueryBuilder<Member, IMemberQueryBuilder>
{
    IMemberQueryBuilder HasEmailAddress(string emailAddress);

    IMemberQueryBuilder InChapter(Guid chapterId);

    IMemberQueryBuilder InChapter(Guid chapterId, MemberChapterQueryOptions options);

    IMemberQueryBuilder IsChapterOwner(Guid chapterId);

    IMemberQueryBuilder Latest(int pageSize);

    IQueryBuilder<MemberWithAvatarDto> WithAvatar();
}