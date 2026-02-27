using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterAdminMemberQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterAdminMember, IChapterAdminMemberQueryBuilder>
{
    IChapterAdminMemberQueryBuilder ForChapter(Guid chapterId);

    IChapterAdminMemberQueryBuilder ForMember(Guid memberId);

    IDeferredQuery<bool> IsAdmin(Guid chapterId, Guid memberId);

    IQueryBuilder<ChapterAdminMemberDto> ToDto();

    IQueryBuilder<ChapterAdminMemberWithAvatarDto> WithAvatar();
}