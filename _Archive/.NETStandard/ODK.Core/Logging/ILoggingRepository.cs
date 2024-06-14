using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Logging
{
    public interface ILoggingRepository
    {
        Task DeleteError(Guid id);

        Task DeleteLogMessage(int id);

        Task DeleteLogMessages(string message);

        Task<Error?> GetError(Guid id);

        Task<IReadOnlyCollection<ErrorProperty>> GetErrorProperties(Guid id);

        Task<IReadOnlyCollection<Error>> GetErrors(int page, int pageSize);

        Task<IReadOnlyCollection<Error>> GetErrors(int page, int pageSize, string exceptionMessage);

        Task<LogMessage?> GetLogMessage(int id);

        Task<IReadOnlyCollection<LogMessage>> GetLogMessages(string level, int page, int pageSize);

        Task<IReadOnlyCollection<LogMessage>> GetLogMessages(string level, int page, int pageSize, string message);

        Task LogError(Error error, IReadOnlyCollection<ErrorProperty> properties);
    }
}
