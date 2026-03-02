using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IMemberChapterRepository : IReadWriteRepository<MemberChapter>
{
    IMemberChapterQueryBuilder Query(PlatformType platform);
}