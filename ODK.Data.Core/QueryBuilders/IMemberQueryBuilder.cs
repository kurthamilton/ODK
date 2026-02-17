using ODK.Core.Members;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberQueryBuilder : IQueryBuilder<Member>
{
    IMemberQueryBuilder Current(Guid chapterId);

    IMemberQueryBuilder InChapter(Guid chapterId);

    IMemberQueryBuilder Latest(int pageSize);

    IMemberQueryBuilder Visible(Guid chapterId);
    
    IQueryBuilder<MemberWithAvatarDto> WithAvatar();
}