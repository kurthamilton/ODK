using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteQuestionRepository : IReadWriteRepository<SiteQuestion>
{
    IDeferredQueryMultiple<SiteQuestion> GetByPlatform(PlatformType platform);

    IDeferredQuery<bool> HasQuestions(PlatformType platform);
}
