using ODK.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberChapterImportQueryBuilder
    : IDatabaseEntityQueryBuilder<MemberChapterImport, IMemberChapterImportQueryBuilder>
{
    /// <summary>Rows that arrived before the given instant - see <see cref="MemberChapterImport.CreatedUtc"/>.</summary>
    IMemberChapterImportQueryBuilder CreatedBefore(DateTime createdBeforeUtc);

    IMemberChapterImportQueryBuilder InChapter(Guid chapterId);
}
