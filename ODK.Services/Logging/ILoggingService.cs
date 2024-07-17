namespace ODK.Services.Logging;

public interface ILoggingService
{
    Task DeleteError(Guid currentMemberId, int logMessageId);

    Task DeleteAllErrors(Guid currentMemberId, int logMessageId);

    Task LogDebug(string message);

    Task LogError(Exception exception, HttpRequest request);

    Task LogError(Exception exception, IDictionary<string, string> data);
}
