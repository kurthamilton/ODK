using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterRepository : WriteRepositoryBase<Chapter>, IChapterRepository
{
    public ChapterRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Chapter> GetAll(PlatformType platform, bool includeUnpublished)
        => Set(platform, includeUnpublished)
            .DeferredMultiple();

    public IDeferredQueryMultiple<Chapter> GetByAdminMemberId(PlatformType platform, Guid memberId)
    {
        var query =
            from chapter in Set(platform, includeUnpublished: true)
            from chapterAdminMember in Set<ChapterAdminMember>()
                .Where(x => x.ChapterId == chapter.Id)
            where chapterAdminMember.MemberId == memberId
            select chapter;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingle<Chapter> GetByEventId(PlatformType platform, Guid eventId)
    {
        var query =
            from chapter in Set(platform, includeUnpublished: true)
            from @event in Set<Event>()
                .Where(x => x.ChapterId == chapter.Id)
            where @event.Id == eventId
            select chapter;

        return query.DeferredSingle();
    }

    public IDeferredQuerySingle<Chapter> GetById(PlatformType platform, Guid id)
        => Set(platform, includeUnpublished: true)
            .Where(x => x.Id == id)
            .DeferredSingle();

    public IDeferredQuerySingleOrDefault<Chapter> GetByIdOrDefault(PlatformType platform, Guid id)
        => Set(platform, includeUnpublished: true)
            .Where(x => x.Id == id)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<Chapter> GetByIds(PlatformType platform, IEnumerable<Guid> ids)
        => Set(platform, includeUnpublished: true)
            .Where(x => ids.Contains(x.Id))
            .DeferredMultiple();

    public IDeferredQueryMultiple<Chapter> GetByMemberId(PlatformType platform, Guid memberId)
    {
        var query =
            from chapter in Set(platform, includeUnpublished: true)
            from memberChapter in Set<MemberChapter>()
                .Where(x => x.ChapterId == chapter.Id)
            where memberChapter.MemberId == memberId
            select chapter;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<Chapter> GetByName(PlatformType platform, string name)
    {
        return Set(platform, includeUnpublished: true)
            .Where(x => x.Name == name)
            .DeferredSingleOrDefault();
    }

    public IDeferredQueryMultiple<Chapter> GetByOwnerId(PlatformType platform, Guid ownerId)
        => Set(platform, includeUnpublished: true)
            .Where(x => x.OwnerId == ownerId)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Chapter> GetBySlug(PlatformType platform, string slug)
        => Set(platform, includeUnpublished: true)
            .Where(x => x.Slug == slug)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<Chapter> GetByTopicGroupId(PlatformType platform, Guid topicGroupId)
    {
        var query =
            from chapter in Set(platform, includeUnpublished: false)
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

    private IQueryable<Chapter> Set(PlatformType platform, bool includeUnpublished)
        => base.Set()
            .ForPlatform(platform, includeUnpublished);
}