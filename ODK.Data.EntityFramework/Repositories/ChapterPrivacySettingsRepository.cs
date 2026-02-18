using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPrivacySettingsRepository :
    WriteRepositoryBase<ChapterPrivacySettings>, IChapterPrivacySettingsRepository
{
    private readonly IChapterEntityRepository<ChapterPrivacySettings> _chapterEntityRepository;

    public ChapterPrivacySettingsRepository(OdkContext context)
        : base(context)
    {
        _chapterEntityRepository = new ChapterEntityRepositoryHelper<ChapterPrivacySettings>(this);
    }

    public IDeferredQuerySingleOrDefault<ChapterPrivacySettings> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault();

    public void Upsert(ChapterPrivacySettings entity, Guid chapterId)
        => _chapterEntityRepository.Upsert(entity, chapterId);
}
