using ODK.Core.Logging;
using ODK.Core.Web;

namespace ODK.Services.Logging;

public interface ILoggingService
{
    Task DeleteError(Guid currentMemberId, Guid id);

    Task DeleteAllErrors(Guid currentMemberId, Guid id);

    Task Error(string message);

    Task Error(string message, Exception exception);

    Task Error(Exception exception, HttpRequest request);

    Task Error(Exception exception, IDictionary<string, string?> properties);

    Task<ErrorDto> GetErrorDto(Guid currentMemberId, Guid errorId);

    Task<IReadOnlyCollection<Error>> GetErrors(Guid currentMemberId, int page, int pageSize);

    bool IgnoreUnknownRequestPath(IHttpRequestContext httpRequestContext);

    Task Info(string message);

    Task Warn(string message);

    Task Warn(string message, IDictionary<string, string?> properties);
}