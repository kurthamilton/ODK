using System.Collections.Concurrent;

namespace ODK.Data.EntityFramework.Caching;

public class EntityCache<TKey, T> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, T> _cache = new ConcurrentDictionary<TKey, T>();
    private readonly Func<T, bool>? _filter;
    private readonly Func<T, TKey> _keyFunc;
    private readonly IDictionary<Guid, HashSet<TKey>> _pendingRemoves = new Dictionary<Guid, HashSet<TKey>>();
    private readonly IDictionary<Guid, HashSet<T>> _pendingUpdates = new Dictionary<Guid, HashSet<T>>();
    
    private bool _allRecords = false;

    public EntityCache(Func<T, TKey> keyFunc, Func<T, bool>? filter = null)
    {
        _filter = filter;
        _keyFunc = keyFunc;
    }    

    public void Clear()
    {
        _allRecords = false;
        _cache.Clear();
    }

    public void CommitPending(Guid sessionKey)
    {
        if (_pendingUpdates.Remove(sessionKey, out var pendingChanges))
        {
            SetSome(pendingChanges);
        }        

        if (_pendingRemoves.Remove(sessionKey, out var pendingRemoves))
        {
            foreach (var key in pendingRemoves)
            {
                Remove(key);
            }
        }
    }    

    public void EndSession(Guid key) => _pendingUpdates.Remove(key);

    public T? Find(Func<T, bool> predicate) => _cache.Values.FirstOrDefault(predicate);

    public Guid GenerateSessionKey() => Guid.NewGuid();

    public T? Get(TKey key)
    {
        _cache.TryGetValue(key, out var value);
        return value;
    }

    public IReadOnlyCollection<T>? GetAll() => _allRecords ? _cache.Values.ToArray() : null;

    public void Remove(TKey key) => _cache.Remove(key, out var _);

    public void RemovePending(Guid sessionKey, T value)
    {
        if (!_pendingRemoves.ContainsKey(sessionKey))
        {
            _pendingRemoves[sessionKey] = new HashSet<TKey>();
        }

        var key = _keyFunc(value);
        _pendingRemoves[sessionKey].Add(key);
    }

    public void Set(T value)
    {
        var key = _keyFunc(value);
        _cache[key] = value;
    }

    public void SetAll(IEnumerable<T> value)
    {
        SetSome(value);
        _allRecords = true;
    }

    public void SetPending(Guid sessionKey, T value)
    {
        if (!_pendingUpdates.ContainsKey(sessionKey))
        {
            _pendingUpdates[sessionKey] = new HashSet<T>();
        }

        _pendingUpdates[sessionKey].Add(value);
    }

    public void SetSome(IEnumerable<T> value)
    {
        foreach (var item in value)
        {
            Set(item);
        }
    }    
}
