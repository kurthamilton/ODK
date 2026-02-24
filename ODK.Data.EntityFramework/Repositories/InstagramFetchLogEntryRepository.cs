using Microsoft.EntityFrameworkCore;
using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class InstagramFetchLogEntryRepository : ReadWriteRepositoryBase<InstagramFetchLogEntry>, IInstagramFetchLogEntryRepository
{
    public InstagramFetchLogEntryRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<InstagramFetchLogEntry> GetLast()
        => Set()
            .OrderByDescending(x => x.CreatedUtc)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<InstagramFetchLogEntry> GetLast(string username)
        => Set()
            .Where(x => x.Username == username)
            .OrderByDescending(x => x.CreatedUtc)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<InstagramFetchLogEntry> GetLastSuccessful()
        => Set()
            .Where(x => x.Success)
            .OrderByDescending(x => x.CreatedUtc)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<InstagramFetchLogEntry> GetRecentSuccessful(int count)
        => Set()
            .Where(x => x.Success)
            .OrderByDescending(x => x.CreatedUtc)
            .Take(count)
            .DeferredMultiple();
}