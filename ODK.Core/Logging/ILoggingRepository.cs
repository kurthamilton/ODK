namespace ODK.Core.Logging;

public interface ILoggingRepository
{
    Task DeleteErrorAsync(Guid id);

    Task DeleteLogMessageAsync(int id);

    Task DeleteLogMessagesAsync(string message);

    Task<Error?> GetErrorAsync(Guid id);

    Task<IReadOnlyCollection<ErrorProperty>> GetErrorPropertiesAsync(Guid id);

    Task<IReadOnlyCollection<Error>> GetErrorsAsync(int page, int pageSize);

    Task<IReadOnlyCollection<Error>> GetErrorsAsync(int page, int pageSize, string exceptionMessage);

    Task<LogMessage?> GetLogMessageAsync(int id);

    Task<IReadOnlyCollection<LogMessage>> GetLogMessagesAsync(string level, int page, int pageSize);

    Task<IReadOnlyCollection<LogMessage>> GetLogMessagesAsync(string level, int page, int pageSize, string message);

    Task LogErrorAsync(Error error, IReadOnlyCollection<ErrorProperty> properties);
}
