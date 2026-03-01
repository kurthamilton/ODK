using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterAdminMemberQueryBuilder
    : DatabaseEntityQueryBuilder<ChapterAdminMember, IChapterAdminMemberQueryBuilder>, IChapterAdminMemberQueryBuilder
{
    public ChapterAdminMemberQueryBuilder(DbContext context, PlatformType platform)
        : base(context, BaseQuery(context, platform))
    {
    }

    protected override IChapterAdminMemberQueryBuilder Builder => this;

    public IChapterAdminMemberQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IChapterAdminMemberQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public IDeferredQuery<bool> IsAdmin(Guid chapterId, Guid memberId)
    {
        var query =
            from member in Set<Member>()
            from adminMember in Query
                .Where(x => x.MemberId == member.Id && x.ChapterId == chapterId)
                .DefaultIfEmpty()
            where
                member.Id == memberId &&
                (member.SiteAdmin || adminMember.MemberId == member.Id)
            select member.Id;

        return query.DeferredAny();
    }

    public IQueryBuilder<ChapterAdminMemberDto> ToDto()
    {
        var query =
            from chapterAdminMember in Query
            from chapter in Set<Chapter>()
                .Where(x => x.Id == chapterAdminMember.ChapterId)
            select new ChapterAdminMemberDto
            {
                Chapter = chapter,
                ChapterAdminMember = chapterAdminMember
            };

        return ProjectTo(query);
    }

    public IQueryBuilder<ChapterAdminMemberWithAvatarDto> WithAvatar()
    {
        var query =
            from adminMember in Query
            from avatar in Set<MemberAvatar>()
                .Where(x => x.MemberId == adminMember.MemberId)
                .DefaultIfEmpty()
            select new ChapterAdminMemberWithAvatarDto
            {
                AvatarVersion = avatar != null ? avatar.VersionInt : null,
                ChapterAdminMember = adminMember
            };

        return ProjectTo(query);
    }

    private static IQueryable<ChapterAdminMember> BaseQuery(DbContext context, PlatformType platform)
    {
        var chapterQuery =
            from chapter in context.Set<Chapter>()
                .ForPlatform(platform, includeUnpublished: true)
            select chapter;

        var query =
            from chapterAdminMember in context.Set<ChapterAdminMember>()
                .Include(x => x.Member)
                .ThenInclude(x => x.Chapters)
            from chapter in chapterQuery
                .Where(x => x.Id == chapterAdminMember.ChapterId)
            select chapterAdminMember;

        return query;
    }
}