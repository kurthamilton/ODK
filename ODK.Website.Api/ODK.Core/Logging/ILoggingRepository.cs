using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Logging
{
    public interface ILoggingRepository
    {
        Task<IReadOnlyCollection<LogMessage>> GetLogMessages(string level, int page, int pageSize);
    }
}
