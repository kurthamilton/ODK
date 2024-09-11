using ODK.Core;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework;

internal class ChapterEntityRepositoryHelper<T> : IChapterEntityRepository<T> where T : IChapterEntity
{
    private readonly IWriteRepository<T> _repository;

    internal ChapterEntityRepositoryHelper(IWriteRepository<T> repository)
    {
        _repository = repository;
    }

    public void Upsert(T entity, Guid chapterId)
    {
        if (entity.ChapterId == default)
        {
            entity.ChapterId = chapterId;
            _repository.Add(entity);
        }
        else
        {
            _repository.Update(entity);
        }
    }
}
