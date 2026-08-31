using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework;

public abstract class RepositoryBase
{
    private readonly DbContext _context;

    protected RepositoryBase(DbContext context)
    {
        _context = context;
        _context.SavedChanges += OnContextSavedChanges;
    }

    public void AddMany<T>(IEnumerable<T> entities) where T : class => _context.Set<T>().AddRange(entities);

    /* Removes the instance EF is already following, where it is following one. Remove attaches what it is
       given, so a second instance of a row already tracked would be rejected - see FindTracked. */
    public void DeleteSingle<T>(T entity) where T : class
        => _context.Set<T>().Remove((T?)FindTracked(entity)?.Entity ?? entity);

    /* Writes onto the instance EF is already following, where it is following one - see FindTracked. */
    public void UpdateSingle<T>(T entity) where T : class
    {
        var tracked = FindTracked(entity);

        if (tracked != null)
        {
            tracked.CurrentValues.SetValues(entity);

            // A row still waiting to be inserted stays an insert; only a loaded one becomes an update.
            if (tracked.State != EntityState.Added)
            {
                tracked.State = EntityState.Modified;
            }

            return;
        }

        /* Entry rather than Attach: Attach walks the graph, so an entity would drag its navigations into
           the change tracker with it - and since reads are not tracked, two rows sharing a related entity
           each carry their own instance of it, which EF rejects. Entry tracks the one entity, which is all
           an update needs: every foreign key in the model is a mapped property, so nothing about the row
           is expressed only through a navigation. */
        _context.Entry(entity).State = EntityState.Modified;
    }

    protected void AddSingle<T>(T entity) where T : class => _context.Set<T>().Add(entity);

    protected virtual void OnCommit()
    {
    }

    protected TBuilder CreateQueryBuilder<TBuilder, T>(Func<DbContext, TBuilder> factory)
        where TBuilder : IQueryBuilder<T>
        where T : class
        => factory(_context);

    protected IQueryable<T> Set<T>() where T : class => _context.Set<T>();

    /* The entry already tracked for this row, where there is one. EF allows one instance per key, and a
       repository cannot know whether the caller has already written this row in the same unit of work - nor,
       once Update takes a clone, that two instances are the same row at all.

       Keyed off the model rather than off IDatabaseEntity, so an entity keyed on anything else is covered
       too - EventTicketSettings is keyed on its event, and would otherwise skip the check entirely.

       This does not reach a duplicate hanging off the entity, which is a separate problem: Remove still
       walks the graph, so deleting two rows that share a related entity attaches it twice. UpdateSingle
       avoids that by not walking it at all. */
    private EntityEntry? FindTracked<T>(T entity) where T : class
    {
        var key = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();

        if (key == null)
        {
            return null;
        }

        var keyValues = new object?[key.Properties.Count];

        for (var i = 0; i < key.Properties.Count; i++)
        {
            var property = key.Properties[i];

            // A shadow property has nothing to read off the instance, so there is no key to look up by.
            var value = property.PropertyInfo?.GetValue(entity) ?? property.FieldInfo?.GetValue(entity);

            if (value == null)
            {
                return null;
            }

            keyValues[i] = value;
        }

        return _context.Set<T>().Local.FindEntryUntyped(keyValues);
    }

    private void OnContextSavedChanges(object? sender, SavedChangesEventArgs e) => OnCommit();
}