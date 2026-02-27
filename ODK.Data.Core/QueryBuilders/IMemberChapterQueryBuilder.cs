using ODK.Core.Members;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberChapterQueryBuilder : IDatabaseEntityQueryBuilder<MemberChapter, IMemberChapterQueryBuilder>
{
    IMemberChapterQueryBuilder ForMember(Guid memberId);

    IQueryBuilder<MemberChapterDto> ToDto();
}