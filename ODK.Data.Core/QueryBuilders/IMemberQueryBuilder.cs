using ODK.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberQueryBuilder : IQueryBuilder<Member>
{
    IMemberQueryBuilder InChapter(Guid chapterId);
}