using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Emails;

namespace ODK.Data.Core.Repositories;

public interface IChapterEmailRepository : IReadWriteRepository<ChapterEmail>
{
    IDeferredQueryMultiple<ChapterEmail> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<ChapterEmail> GetByChapterId(Guid chapterId, EmailType type);

    /// <summary>
    /// Everything a send of <paramref name="type"/> renders from, in one query: the layout, the email for
    /// the type, and the group's override of either.
    /// </summary>
    /// <param name="chapterId">
    /// Null for a send that belongs to no group, which leaves both overrides null.
    /// </param>
    IDeferredQuerySingle<ChapterEmailDto> GetDto(Guid? chapterId, EmailType type);
}
