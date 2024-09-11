using ODK.Core;

namespace ODK.Data.Core.Repositories;

public interface IChapterEntityRepository<T> where T : IChapterEntity
{
    void Upsert(T entity, Guid chapterId);
}
