using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Topics;
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

    public IDeferredQueryMultiple<ChapterTopicDto> GetDtosByChapterIds(IEnumerable<Guid> chapterIds)
    {
        var query =
            from chapterTopic in Set()
            from topic in Set<Topic>()
                .Include(x => x.TopicGroup)
            where topic.Id == chapterTopic.TopicId
                && chapterIds.Contains(chapterTopic.ChapterId)
            select new ChapterTopicDto
            {
                ChapterId = chapterTopic.ChapterId,
                Topic = topic
            };

        return query.DeferredMultiple();
    }

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

    protected override IQueryable<ChapterTopic> Set() => base.Set()
        .Include(x => x.Topic)
        .ThenInclude(x => x.TopicGroup);
}
