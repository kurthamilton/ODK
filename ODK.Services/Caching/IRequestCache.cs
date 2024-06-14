using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Caching;

public interface IRequestCache
{
    Task<Chapter?> GetChapter(Guid chapterId);

    Task<Chapter?> GetChapter(string name);

    Task<IReadOnlyCollection<Chapter>> GetChapters();

    Task<Member?> GetMember(Guid memberId);
}
