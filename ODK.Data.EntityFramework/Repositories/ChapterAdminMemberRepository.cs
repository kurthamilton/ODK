using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterAdminMemberRepository
    : WriteRepositoryBase<ChapterAdminMember>, IChapterAdminMemberRepository
{
    public ChapterAdminMemberRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterAdminMember> GetByChapterId(PlatformType platform, Guid chapterId)
        => Query(platform)
            .ForChapter(chapterId)
            .GetAll();

    public IDeferredQueryMultiple<ChapterAdminMember> GetByMemberId(PlatformType platform, Guid memberId)
        => Query(platform)
            .ForMember(memberId)
            .GetAll();

    public IDeferredQuerySingleOrDefault<ChapterAdminMember> GetByMemberId(
        PlatformType platform, Guid memberId, Guid chapterId)
        => Query(platform)
            .ForMember(memberId)
            .ForChapter(chapterId)
            .GetSingleOrDefault();

    public IDeferredQueryMultiple<ChapterAdminMemberWithAvatarDto> GetAdminMembersWithAvatarsByChapterId(
        PlatformType platform, Guid chapterId)
        => Query(platform)
            .ForChapter(chapterId)
            .WithAvatar()
            .GetAll();

    public IDeferredQuery<bool> IsAdmin(PlatformType platform, Guid chapterId, Guid memberId)
        => Query(platform)
            .IsAdmin(chapterId, memberId);

    public IChapterAdminMemberQueryBuilder Query(PlatformType platform)
        => CreateQueryBuilder<IChapterAdminMemberQueryBuilder>(
            context => new ChapterAdminMemberQueryBuilder(context, platform));
}