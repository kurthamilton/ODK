using System;
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

        public async Task DeleteError(Guid id)
        {
            await Context
                .Delete<Error>()
                .Where(x => x.Id).EqualTo(id)
                .ExecuteAsync();
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

        public async Task<Error> GetError(Guid id)
        {
            return await Context
                .Select<Error>()
                .Where(x => x.Id).EqualTo(id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<ErrorProperty>> GetErrorProperties(Guid id)
        {
            return await Context
                .Select<ErrorProperty>()
                .Where(x => x.ErrorId).EqualTo(id)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Error>> GetErrors(int page, int pageSize)
        {
            return await Context
                .Select<Error>()
                .OrderBy(x => x.CreatedDate, SqlSortDirection.Descending)
                .Page(page, pageSize)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Error>> GetErrors(int page, int pageSize, string exceptionMessage)
        {
            return await Context
                .Select<Error>()
                .Where(x => x.ExceptionMessage).EqualTo(exceptionMessage)
                .OrderBy(x => x.CreatedDate, SqlSortDirection.Descending)
                .Page(page, pageSize)
                .ToArrayAsync();
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

        public async Task LogError(Error error, IReadOnlyCollection<ErrorProperty> properties)
        {
            var logMessage = new LogMessage(0, "Error", error.ExceptionMessage, DateTime.UtcNow, error.ExceptionType.ToString(), "");

            await Context
                .Insert(logMessage)
                .ExecuteAsync();

            // foreach (ErrorProperty property in properties)
            // {
            //     await Context
            //         .Insert(property)
            //         .ExecuteAsync();
            // }
        }

        public async Task LogError(LogMessage message)
        {
            await Context
                .Insert(message)
                .ExecuteAsync();
        }
    }
}
