using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.QueryBuilders.Members;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.QueryBuilders;

/* This is a prototype for how repository methods might be replaced by query composition.
 * Pros: 
 *  - composable queries
 *  - fewer methods (e.g. GetByChapter, GetLatestByChapter, GetWithAvatarByChapter are all v similar
 *  - high code reuse
 * Cons:
 *  - certain filter chains that are currently encapsulated in one method would benefit from a higher-order function, which
 *    could lead to method explosion
 */
public class MemberQueryBuilder : DatabaseEntityQueryBuilder<Member, IMemberQueryBuilder>, IMemberQueryBuilder
{
    internal MemberQueryBuilder(OdkContext context)
        : base(context)
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

    protected override IQueryable<Member> Set(OdkContext context)
        => base.Set(context)
            .Include(x => x.Chapters);
}