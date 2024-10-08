﻿using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPrivacySettingsRepository :
    WriteRepositoryBase<ChapterPrivacySettings>, IChapterPrivacySettingsRepository
{
    private static readonly EntityCache<Guid, ChapterPrivacySettings> _cache = new EntityCache<Guid, ChapterPrivacySettings>(x => x.ChapterId);

    private readonly IChapterEntityRepository<ChapterPrivacySettings> _chapterEntityRepository;

    public ChapterPrivacySettingsRepository(OdkContext context) 
        : base(context)
    {
        _chapterEntityRepository = new ChapterEntityRepositoryHelper<ChapterPrivacySettings>(this);
    }

    public IDeferredQuerySingleOrDefault<ChapterPrivacySettings> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault(
            () => _cache.Get(chapterId),
            _cache.Set);

    public void Upsert(ChapterPrivacySettings entity, Guid chapterId)
        => _chapterEntityRepository.Upsert(entity, chapterId);
}
