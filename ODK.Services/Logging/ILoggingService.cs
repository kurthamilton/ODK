using ODK.Core.Logging;

namespace ODK.Services.Logging;

public interface ILoggingService
{
    Task DeleteError(Guid currentMemberId, Guid id);

    Task DeleteAllErrors(Guid currentMemberId, Guid id);

    Task<ErrorDto> GetErrorDto(Guid currentMemberId, Guid errorId);

    Task<IReadOnlyCollection<Error>> GetErrors(Guid currentMemberId, int page, int pageSize);

    Task LogDebug(string message);

    Task LogError(Exception exception, HttpRequest request);

    Task LogError(Exception exception, IDictionary<string, string> data);
}
