using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Topics;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterQueryBuilder : DatabaseEntityQueryBuilder<Chapter, IChapterQueryBuilder>, IChapterQueryBuilder
{
    public ChapterQueryBuilder(OdkContext context, PlatformType platform, bool includeUnpublished) 
        : base(context, BaseQuery(context, platform, includeUnpublished))
    {
    }

    protected override IChapterQueryBuilder Builder => this;

    public IChapterQueryBuilder Approved()
    {
        Query = Query.Where(x => x.ApprovedUtc != null);
        return this;
    }

    public IChapterQueryBuilder ForAdminMember(Guid memberId)
    {
        Query =
            from chapter in Query
            from chapterAdminMember in Set<ChapterAdminMember>()
                .Where(x => x.ChapterId == chapter.Id)
            where chapterAdminMember.MemberId == memberId
            select chapter;
        return this;
    }

    public IChapterQueryBuilder ForEvent(Guid eventId)
    {
        Query =
            from chapter in Query
            from @event in Set<Event>()
                .Where(x => x.ChapterId == chapter.Id)
            where @event.Id == eventId
            select chapter;
        return this;
    }

    public IChapterQueryBuilder ForMember(Guid memberId)
    {
        Query = 
            from chapter in Query
            from memberChapter in Set<MemberChapter>()
                .Where(x => x.ChapterId == chapter.Id)
            where memberChapter.MemberId == memberId
            select chapter;
        return this;
    }

    public IChapterQueryBuilder ForName(string name)
    {
        Query = Query.Where(x => x.Name == name);
        return this;
    }

    public IChapterQueryBuilder ForOwner(Guid ownerId)
    {
        Query = Query.Where(x => x.OwnerId == ownerId);
        return this;
    }

    public IChapterQueryBuilder ForSlug(string slug)
    {
        Query = Query.Where(x => x.Slug == slug);
        return this;
    }

    public IMemberQueryBuilder Owner()
    {
        var query =
            from chapter in Query
            from member in Set<Member>()
                .Where(x => x.Id == chapter.OwnerId)
            select member;
        return CreateQueryBuilder<IMemberQueryBuilder, Member>(
            context => new MemberQueryBuilder(context, query));
    }

    public IQueryBuilder<ChapterSearchResultDto> Search(ChapterSearchCriteria criteria)
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
            from chapter in Query
            from image in Set<ChapterImage>()
                .Where(x => x.ChapterId == chapter.Id)
                .DefaultIfEmpty()
                .Select(x => x != null ? new ChapterImageVersionDto
                {
                    Version = x.VersionInt
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

        return ProjectTo(query);
    }

    public IQueryBuilder<ChapterDto> ToChapterDto()
    {
        var query =
            from chapter in Query
            from image in Set<ChapterImage>()
                .Where(x => x.ChapterId == chapter.Id)
                .DefaultIfEmpty()
                .Select(x => x != null ? new ChapterImageVersionDto
                {
                    Version = x.VersionInt
                } : null)
            from texts in Set<ChapterTexts>()
                .Where(x => x.ChapterId == chapter.Id)
                .DefaultIfEmpty()
            select new ChapterDto
            {
                Chapter = chapter,
                Image = image,
                Texts = texts
            };

        return ProjectTo(query);
    }

    private static IQueryable<Chapter> BaseQuery(OdkContext context, PlatformType platform, bool includeUnpublished)
        => context
            .Set<Chapter>()
            .ForPlatform(platform, includeUnpublished);
}
