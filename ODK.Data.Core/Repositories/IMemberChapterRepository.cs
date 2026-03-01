using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IMemberChapterRepository : IReadWriteRepository<MemberChapter, IMemberChapterQueryBuilder>
{
}