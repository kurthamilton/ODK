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

        public async Task DeleteLogMessage(int id)
        {
            await Context
                .Delete<LogMessage>()
                .Where(x => x.Id).EqualTo(id)
                .ExecuteAsync();
        }

        public async Task DeleteLogMessages(string message)
        {
            await Context
                .Delete<LogMessage>()
                .Where(x => x.Message).EqualTo(message)
                .ExecuteAsync();
        }

        public async Task<LogMessage> GetLogMessage(int id)
        {
            return await Context
                .Select<LogMessage>()
                .Where(x => x.Id).EqualTo(id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<LogMessage>> GetLogMessages(string level, int page, int pageSize)
        {
            return await GetLogMessages(level, page, pageSize, null);
        }

        public async Task<IReadOnlyCollection<LogMessage>> GetLogMessages(string level, int page, int pageSize,
            string message)
        {
            return await Context
                .Select<LogMessage>()
                .ConditionalWhere(x => x.Level, !string.IsNullOrEmpty(level)).EqualTo(level)
                .ConditionalWhere(x => x.Message, !string.IsNullOrEmpty(message)).EqualTo(message)
                .OrderBy(x => x.TimeStamp, SqlSortDirection.Descending)
                .Page(page, pageSize)
                .ToArrayAsync();
        }
    }
}
