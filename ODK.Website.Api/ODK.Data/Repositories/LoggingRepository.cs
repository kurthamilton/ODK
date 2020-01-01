using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Logging;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class LoggingRepository : RepositoryBase, ILoggingRepository
    {
        public LoggingRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task<IReadOnlyCollection<LogMessage>> GetLogMessages(string level, int page, int pageSize)
        {
            return await Context
                .Select<LogMessage>()
                .OrderBy(x => x.TimeStamp, SqlSortDirection.Descending)
                .Page(page, pageSize)
                .ConditionalWhere(x => x.Level, !string.IsNullOrEmpty(level)).EqualTo(level)
                .ToArrayAsync();
        }
    }
}
