using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IInstagramFetchLogEntryRepository : IReadWriteRepository<InstagramFetchLogEntry>
{
    IDeferredQuerySingleOrDefault<InstagramFetchLogEntry> GetLast();

    IDeferredQuerySingleOrDefault<InstagramFetchLogEntry> GetLast(string username);

    IDeferredQuerySingleOrDefault<InstagramFetchLogEntry> GetLastSuccessful();

    IDeferredQueryMultiple<InstagramFetchLogEntry> GetRecentSuccessful(int count);
}