using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.QueryBuilders.Members;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberQueryBuilder : DatabaseEntityQueryBuilder<Member, IMemberQueryBuilder>, IMemberQueryBuilder
{
    internal MemberQueryBuilder(DbContext context)
        : base(context, BaseQuery(context))
    {
    }

    internal MemberQueryBuilder(DbContext context, IQueryable<Member> query)
        : base(context, query)
    {
    }

    protected override IMemberQueryBuilder Builder => this;

    public IMemberQueryBuilder HasEmailAddress(string emailAddress)
    {
        Query = Query.Where(x => x.EmailAddress == emailAddress);
        return this;
    }

    public IMemberQueryBuilder InChapter(Guid chapterId)
        => InChapter(chapterId, new());

    public IMemberQueryBuilder InChapter(Guid chapterId, MemberChapterQueryOptions options)
    {
        Query = Query.InChapter(chapterId, options);
        return this;
    }

    public IMemberQueryBuilder IsChapterOwner(Guid chapterId)
    {
        Query =
            from member in Query
            from chapter in Set<Chapter>()
                .Where(x => x.OwnerId == member.Id)
            where chapter.Id == chapterId
            select member;
        return this;
    }

    public IMemberQueryBuilder Latest(int pageSize)
    {
        Query = Query
            .OrderByDescending(x => x.CreatedUtc)
            .Take(pageSize);
        return this;
    }

    public IQueryBuilder<MemberWithAvatarDto> WithAvatar()
    {
        var query =
            from member in Query
            from avatar in Set<MemberAvatar>()
                .Where(x => x.MemberId == member.Id)
                .DefaultIfEmpty()
            select new MemberWithAvatarDto
            {
                AvatarVersion = avatar != null ? avatar.VersionInt : null,
                Member = member
            };

        return ProjectTo(query);
    }

    private static IQueryable<Member> BaseQuery(DbContext context)
        => context.Set<Member>()
            .Include(x => x.Chapters);
}