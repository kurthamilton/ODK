using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Topics;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterRepository : WriteRepositoryBase<Chapter>, IChapterRepository
{
    private readonly OdkContext _context;

    public ChapterRepository(OdkContext context)
        : base(context)
    {
        _context = context;
    }

    public IDeferredQueryMultiple<Chapter> GetAll(PlatformType platform, bool includeUnpublished)
        => Set(platform, includeUnpublished)
            .DeferredMultiple();

    public IDeferredQueryMultiple<Chapter> GetApproved(PlatformType platform)
        => Set(platform, includeUnpublished: false)
            .Where(x => x.ApprovedUtc != null)
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

    public IDeferredQuery<bool> NameExists(string name)
        => Set(PlatformType.Default, includeUnpublished: true)
            .Where(x => x.Name == name)
            .DeferredAny();

    public IDeferredQueryMultiple<ChapterSearchResultDto> Search(PlatformType platform, ChapterSearchCriteria criteria)
    {
        var topicQuery =
            from topic in Set<Topic>()
            select topic;

        if (criteria.TopicGroupNames != null)
        {
            topicQuery =
                from topic in topicQuery
                from topicGroup in Set<TopicGroup>()
                    .Where(x => x.Id == topic.TopicGroupId)
                where criteria.TopicGroupNames.Contains(topicGroup.Name)
                select topic;
        }

        var locationQuery = criteria.Distance != null
            ? Set<ChapterLocation>()
                .WithinDistance(criteria.Distance.Location, criteria.Distance.DistanceMetres)
            : Set<ChapterLocation>()
                .DefaultIfEmpty();

        var query =
            from chapter in Set(platform, includeUnpublished: false)
            from image in Set<ChapterImage>()
                .Where(x => x.ChapterId == chapter.Id)
                .DefaultIfEmpty()
                .Select(x => x != null ? new ChapterImageMetadataDto
                {
                    ChapterId = x.ChapterId,
                    MimeType = x.MimeType
                } : null)
            from texts in Set<ChapterTexts>()
                .Where(x => x.ChapterId == chapter.Id)
                .DefaultIfEmpty()
            from location in locationQuery
                .Where(x => x == null || x.ChapterId == chapter.Id)
            join chapterTopic in Set<ChapterTopic>()
                on chapter.Id equals chapterTopic.ChapterId into chapterTopics
            select new ChapterSearchResultDto
            {
                Chapter = chapter,
                Image = image,
                Location = location,
                Texts = texts,
                Topics = (
                    from chapterTopic in chapterTopics
                    from topic in topicQuery
                        .Where(x => x.Id == chapterTopic.TopicId)
                    select topic).ToList()
            };

        return query
            .DeferredMultiple();
    }

    public IDeferredQuery<bool> SlugExists(string slug)
        => Set(PlatformType.Default, includeUnpublished: true)
            .Where(x => x.Slug == slug)
            .DeferredAny();

    private IQueryable<Chapter> Set(PlatformType platform, bool includeUnpublished)
        => Set()
            .ForPlatform(platform, includeUnpublished);
}