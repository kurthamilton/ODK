using Microsoft.EntityFrameworkCore;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework;

public abstract class RepositoryBase
{
    private readonly OdkContext _context;

    protected RepositoryBase(OdkContext context)
    {
        _context = context;
        _context.SavedChanges += OnContextSavedChanges;
    }    

    protected void AddSingle<T>(T entity) where T : class => _context.Set<T>().Add(entity);

    public void AddMany<T>(IEnumerable<T> entities) where T : class => _context.Set<T>().AddRange(entities);

    public void DeleteSingle<T>(T entity) where T : class => _context.Set<T>().Remove(entity);

    public void UpdateSingle<T>(T entity) where T : class
    {
        _context.Set<T>().Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    protected virtual void OnCommit()
    {
    }

    protected TBuilder CreateQueryBuilder<TBuilder, T>(Func<OdkContext, TBuilder> factory)
        where TBuilder : IQueryBuilder<T>
        where T : class
        => factory(_context);

    protected IQueryable<T> Set<T>() where T : class => _context.Set<T>();

    private void OnContextSavedChanges(object? sender, SavedChangesEventArgs e) => OnCommit();
}