using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterTopicRepository : WriteRepositoryBase<ChapterTopic>, IChapterTopicRepository
{
    public ChapterTopicRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterTopic> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterTopic> GetByChapterIds(IEnumerable<Guid> chapterIds) => Set()
        .Where(x => chapterIds.Contains(x.ChapterId))
        .DeferredMultiple();

    public int Merge(IEnumerable<ChapterTopic> existing, Guid chapterId, IEnumerable<Guid> topicIds)
    {
        var changes = 0;

        var existingDictionary = existing
            .ToDictionary(x => x.TopicId);

        foreach (var topicId in topicIds)
        {
            if (existingDictionary.ContainsKey(topicId))
            {
                continue;
            }

            Add(new ChapterTopic
            {
                ChapterId = chapterId,
                TopicId = topicId
            });

            changes++;
        }

        foreach (var existingChapterTopic in existing)
        {
            if (topicIds.Contains(existingChapterTopic.TopicId))
            {
                continue;
            }

            Delete(existingChapterTopic);
            changes++;
        }

        return changes;
    }
}
