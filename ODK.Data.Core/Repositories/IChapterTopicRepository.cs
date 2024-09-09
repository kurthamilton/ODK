using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterTopicRepository : IWriteRepository<ChapterTopic>
{
    IDeferredQueryMultiple<ChapterTopic> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<ChapterTopic> GetByChapterIds(IEnumerable<Guid> chapterIds);

    int Merge(IEnumerable<ChapterTopic> existing, Guid chapterId, IEnumerable<Guid> topicIds);
}
