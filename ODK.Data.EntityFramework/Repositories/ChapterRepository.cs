using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterRepository : ReadWriteRepositoryBase<Chapter>, IChapterRepository
{
    private readonly PlatformType _platform;

    public ChapterRepository(OdkContext context, IPlatformProvider platformProvider)
        : base(context)
    {
        _platform = platformProvider.GetPlatform();
    }

    public IDeferredQueryMultiple<Chapter> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQueryMultiple<Chapter> GetByMemberId(Guid memberId)
    {
        var query =
            from chapter in Set()
            from memberChapter in Set<MemberChapter>()
            where memberChapter.MemberId == memberId
                && memberChapter.ChapterId == chapter.Id
            select chapter;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<Chapter> GetByName(string name)
    {
        return Set()
            .Where(x => x.Name == name)
            .DeferredSingleOrDefault();
    }

    public IDeferredQueryMultiple<Chapter> GetByOwnerId(Guid ownerId)
        => Set()
            .Where(x => x.OwnerId == ownerId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<Chapter> GetByPlatform(PlatformType platform)
        => Set()
            .Where(x => x.Platform == platform)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Chapter> GetBySlug(string slug)
        => Set()
            .Where(x => x.Slug == slug)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<Chapter> GetByTopicGroupId(Guid topicGroupId)
    {
        var query =
            from chapter in Set()
            where
            (
                from chapterTopic in Set<ChapterTopic>()
                from topic in Set<Topic>()
                where chapterTopic.ChapterId == chapter.Id &&
                    topic.Id == chapterTopic.TopicId &&
                    topic.TopicGroupId == topicGroupId
                select 1
            ).Any()
            select chapter;

        return query.DeferredMultiple();
    }

    public override void Update(Chapter entity)
    {
        var clone = entity.Clone();
        if (clone.Platform == PlatformType.DrunkenKnitwits && !clone.Name.EndsWith(Chapter.DrunkenKnitwitsSuffix))
        {
            clone.Name += Chapter.DrunkenKnitwitsSuffix;
        }

        base.Update(clone);
    }

    protected override IQueryable<Chapter> Set() => base.Set()
        .ConditionalWhere(x => x.Platform == _platform, _platform != PlatformType.Default);
}