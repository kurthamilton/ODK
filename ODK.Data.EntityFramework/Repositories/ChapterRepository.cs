﻿using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterRepository : CachingReadWriteRepositoryBase<Chapter>, IChapterRepository
{
    private static readonly EntityCache<Guid, Chapter> _cache = new DatabaseEntityCache<Chapter>();

    private readonly PlatformType _platform;

    public ChapterRepository(OdkContext context, IPlatformProvider platformProvider)
        : base(context, _cache)
    {
        _platform = platformProvider.GetPlatform();
    }

    public IDeferredQueryMultiple<Chapter> GetAll() => Set()
        .DeferredMultiple(           
            _cache.GetAll,
            _cache.SetAll);    

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

    public IDeferredQuerySingleOrDefault<Chapter> GetByName(string name) => Set()
        .Where(x => x.Name == name)
        .DeferredSingleOrDefault(
            () => _cache.Find(x => string.Equals(
                x.Name,
                name,
                StringComparison.InvariantCultureIgnoreCase)),
            _cache.Set,
            _cache.SetAll);

    public IDeferredQueryMultiple<Chapter> GetByOwnerId(Guid ownerId) => Set()
        .Where(x => x.OwnerId == ownerId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Chapter> GetBySlug(string slug) => Set()
        .Where(x => x.Slug == slug)
        .DeferredSingleOrDefault(
            () => _cache.Find(x => string.Equals(x.Slug, slug, StringComparison.InvariantCultureIgnoreCase)),
            _cache.Set,
            _cache.SetAll);

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
        if (clone.Platform == PlatformType.DrunkenKnitwits && !clone.Name.EndsWith(" Drunken Knitwits"))
        {
            clone.Name += " Drunken Knitwits";
        }

        base.Update(clone);
    }

    protected override void PreCommit(IEnumerable<Chapter> pending)
    {
        base.PreCommit(pending);

        foreach (var chapter in pending)
        {
            if (_platform == PlatformType.DrunkenKnitwits &&
                chapter.Platform == PlatformType.DrunkenKnitwits)
            {
                chapter.Name = chapter.Name.Replace(" Drunken Knitwits", "");
            }
        }
    }

    protected override IQueryable<Chapter> Set() => base.Set()
        .ConditionalWhere(x => x.Platform == _platform, _platform != PlatformType.Default)
        .ToPlatformChapters(_platform);    
}
