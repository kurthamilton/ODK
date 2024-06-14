using ODK.Core.Logging;

namespace ODK.Services.Logging;

public interface ILoggingService
{
    Task DeleteError(Guid currentMemberId, int logMessageId);

    Task DeleteAllErrors(Guid currentMemberId, int logMessageId);

    Task<LogMessage?> GetErrorMessage(Guid currentMemberId, int logMessageId);

    Task<IReadOnlyCollection<LogMessage>> GetErrorMessages(Guid currentMemberId);

    Task<IReadOnlyCollection<LogMessage>> GetLogMessages(Guid currentMemberId, string level, int page, int pageSize);

    Task<IReadOnlyCollection<LogMessage>> GetSimilarErrorMessages(Guid currentMemberId, LogMessage logMessage);

    Task LogDebug(string message);

    Task LogError(Exception exception, HttpRequest request);

    Task LogError(Exception exception, IDictionary<string, string> data);
}
