using Microsoft.EntityFrameworkCore;

namespace ODK.Data.EntityFramework;
public abstract class RepositoryBase
{
    private readonly DbContext _context;

    protected RepositoryBase(OdkContext context)
    {        
        _context = context;
        _context.SavedChanges += OnContextSavedChanges;
    }    

    protected void AddSingle<T>(T entity) where T : class => _context.Set<T>().Add(entity);

    public void AddMany<T>(IEnumerable<T> entities) where T : class => _context.Set<T>().AddRange(entities);

    public void DeleteSingle<T>(T entity) where T : class => _context.Set<T>().Remove(entity);

    public void UpdateSingle<T>(T entity) where T : class => _context.Set<T>().Update(entity);

    protected virtual void OnCommit()
    {        
    }

    protected IQueryable<T> Set<T>() where T : class => _context.Set<T>();

    private void OnContextSavedChanges(object? sender, SavedChangesEventArgs e) => OnCommit();
}
