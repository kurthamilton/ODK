using Microsoft.EntityFrameworkCore;
using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class NewChapterTopicRepository : ReadWriteRepositoryBase<NewChapterTopic>, INewChapterTopicRepository
{
    public NewChapterTopicRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<NewChapterTopic> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQueryMultiple<NewChapterTopic> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();
}