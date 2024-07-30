using ODK.Core;

namespace ODK.Data.EntityFramework.Caching;

public class DatabaseEntityCache<T> : EntityCache<Guid, T> where T : IDatabaseEntity
{    
    public DatabaseEntityCache()
        : base(x => x.Id)
    {        
    }       
}
