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

public class ChapterRepository : ReadWriteRepositoryBase<Chapter>, IChapterRepository
{
    private readonly PlatformType _platform;

    public ChapterRepository(OdkContext context, IPlatformProvider platformProvider)
        : base(context)
    {
        _platform = platformProvider.GetPlatform();
    }

    public IDeferredQueryMultiple<Chapter> GetAll(PlatformType platform)
        => Set(platform)
            .DeferredMultiple();

    public IDeferredQueryMultiple<Chapter> GetByAdminMemberId(PlatformType platform, Guid memberId)
    {
        var query =
            from chapter in Set(platform)
            from chapterAdminMember in Set<ChapterAdminMember>()
                .Where(x => x.ChapterId == chapter.Id)
            where chapterAdminMember.MemberId == memberId
            select chapter;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingle<Chapter> GetByEventId(Guid eventId)
    {
        var query =
            from chapter in Set()
            from @event in Set<Event>()
                .Where(x => x.ChapterId == chapter.Id)
            where @event.Id == eventId
            select chapter;

        return query.DeferredSingle();
    }

    public IDeferredQueryMultiple<Chapter> GetByMemberId(Guid memberId)
    {
        var query =
            from chapter in Set()
            from memberChapter in Set<MemberChapter>()
                .Where(x => x.ChapterId == chapter.Id)
            where memberChapter.MemberId == memberId
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

    public IDeferredQuerySingleOrDefault<Chapter> GetBySlug(string slug)
        => Set()
            .Where(x => x.Slug == slug)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<Chapter> GetByTopicGroupId(PlatformType platform, Guid topicGroupId)
    {
        var query =
            from chapter in Set(platform)
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

    protected override IQueryable<Chapter> Set() => Set(_platform);

    private IQueryable<Chapter> Set(PlatformType platform)
        => base.Set()
            .ForPlatform(platform);
}