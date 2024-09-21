using ODK.Core.Topics;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface INewChapterTopicRepository : IReadWriteRepository<NewChapterTopic>
{
    public IDeferredQueryMultiple<NewChapterTopic> GetAll();

    public IDeferredQueryMultiple<NewChapterTopic> GetByChapterId(Guid chapterId);
}
