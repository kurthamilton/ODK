using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Logging
{
    public interface ILoggingRepository
    {
        Task DeleteLogMessage(int id);

        Task DeleteLogMessages(string message);

        Task<LogMessage> GetLogMessage(int id);

        Task<IReadOnlyCollection<LogMessage>> GetLogMessages(string level, int page, int pageSize);

        Task<IReadOnlyCollection<LogMessage>> GetLogMessages(string level, int page, int pageSize, string message);
    }
}
