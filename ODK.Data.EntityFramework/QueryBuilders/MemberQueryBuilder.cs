using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberQueryBuilder : QueryBuilder<Member>, IMemberQueryBuilder
{
    internal MemberQueryBuilder(OdkContext context)
        : base(context)
    {
    }

    public IMemberQueryBuilder Current(Guid chapterId)
    {
        Query = Query.Current(chapterId);
        return this;
    }

    public IMemberQueryBuilder InChapter(Guid chapterId)
    {
        Query = Query.InChapter(chapterId);
        return this;
    }

    public IMemberQueryBuilder Latest(int pageSize)
    {
        Query = Query
            .OrderByDescending(x => x.CreatedUtc)
            .Take(pageSize);
        return this;
    }

    public IMemberQueryBuilder Visible(Guid chapterId)
    {
        Query = Query.Visible(chapterId);
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

    protected override IQueryable<Member> Set()
        => Query.Include(x => x.Chapters);
}