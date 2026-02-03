using ODK.Core.Logging;
using ODK.Core.Web;

namespace ODK.Services.Logging;

public interface ILoggingService
{
    Task DeleteError(MemberServiceRequest request, Guid id);

    Task DeleteAllErrors(MemberServiceRequest request, Guid id);

    Task Error(string message);

    Task Error(string message, Exception exception);

    Task Error(Exception exception, HttpRequest request);

    Task Error(Exception exception, IDictionary<string, string?> properties);

    Task<ErrorDto> GetErrorDto(MemberServiceRequest request, Guid errorId);

    Task<IReadOnlyCollection<Error>> GetErrors(MemberServiceRequest request, int page, int pageSize);

    bool IgnoreUnknownRequestPath(IHttpRequestContext httpRequestContext);

    Task Info(string message);

    Task Warn(string message);

    Task Warn(string message, IDictionary<string, string?> properties);
}