using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Logging;

namespace ODK.Services.Logging
{
    public interface ILoggingService
    {
        Task<IReadOnlyCollection<LogMessage>> GetLogMessages(Guid currentMemberId, string level, int page, int pageSize);

        Task LogDebug(string message);

        Task LogError(Exception exception, string message);
    }
}
