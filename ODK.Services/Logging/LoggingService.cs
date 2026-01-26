using System.Text.RegularExpressions;
using ODK.Core.Logging;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace ODK.Services.Logging;

public class LoggingService : OdkAdminServiceBase, ILoggingService
{
    private readonly ILogger _logger;
    private readonly LoggingServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public LoggingService(
        ILogger logger,
        IUnitOfWorkFactory unitOfWorkFactory,
        IUnitOfWork unitOfWork,
        LoggingServiceSettings settings)
        : base(unitOfWork)
    {
        _logger = logger;
        _settings = settings;
        _unitOfWork = unitOfWork;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task DeleteError(Guid currentMemberId, Guid id)
    {
        var error = await GetSiteAdminRestrictedContent(currentMemberId,
            x => x.ErrorRepository.GetById(id));

        _unitOfWork.ErrorRepository.Delete(error);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAllErrors(Guid currentMemberId, Guid id)
    {
        var error = await GetSiteAdminRestrictedContent(currentMemberId,
            x => x.ErrorRepository.GetById(id));

        throw new NotImplementedException();
    }

    public Task Error(string message)
        => Log(LogEventLevel.Error, message);

    public Task Error(string message, Exception exception)
        => Log(LogEventLevel.Error, message, exception: exception);

    public async Task Error(Exception exception, HttpRequest request)
    {
        var properties = new Dictionary<string, string?>
        {
            { "REQUEST.URL", request.Url },
            { "REQUEST.METHOD", request.Method },
            { "REQUEST.USERNAME", request.Username },
            { "EXCEPTION.STACKTRACE", exception.StackTrace?.Replace(Environment.NewLine, "<br>") }
        };

        foreach (string key in request.Headers.Keys)
        {
            properties[$"REQUEST.HEADER.{key.ToUpperInvariant()}"] = request.Headers[key];
        }

        foreach (string key in request.Form.Keys)
        {
            properties[$"REQUEST.FORM.{key.ToUpperInvariant()}"] = request.Form[key];
        }

        var innerException = exception.InnerException;
        var innerExceptionCount = 0;
        while (innerException != null)
        {
            properties[$"EXCEPTION.INNEREXCEPTION[{innerExceptionCount}].TYPE"] = innerException.GetType().Name;
            properties[$"EXCEPTION.INNEREXCEPTION[{innerExceptionCount}].MESSAGE"] = innerException.Message;

            innerException = innerException.InnerException;
            innerExceptionCount++;
        }

        await Log(
            LogEventLevel.Error,
            exception.Message,
            properties,
            exception);

        // Create new unit of work to avoid re-instigating any previous context errors
        var unitOfWork = _unitOfWorkFactory.Create();

        var error = Core.Logging.Error.FromException(exception);
        unitOfWork.ErrorRepository.Add(error);

        var errorProperties = properties
            .Select(x => new ErrorProperty
            {
                ErrorId = error.Id,
                Name = x.Key,
                Value = x.Value ?? string.Empty
            })
            .ToArray();
        unitOfWork.ErrorPropertyRepository.AddMany(errorProperties);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task Error(Exception exception, IDictionary<string, string?> properties)
    {
        await Log(LogEventLevel.Error, exception.Message, properties, exception);

        // Create new unit of work to avoid re-instigating any previous context errors
        var unitOfWork = _unitOfWorkFactory.Create();

        var error = Core.Logging.Error.FromException(exception);
        unitOfWork.ErrorRepository.Add(error);

        var errorProperties = properties
            .Select(x => new ErrorProperty
            {
                ErrorId = error.Id,
                Name = x.Key,
                Value = x.Value ?? string.Empty
            })
            .ToArray();

        unitOfWork.ErrorPropertyRepository.AddMany(errorProperties);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<ErrorDto> GetErrorDto(Guid currentMemberId, Guid errorId)
    {
        var (error, properties) = await GetSiteAdminRestrictedContent(currentMemberId,
            x => x.ErrorRepository.GetById(errorId),
            x => x.ErrorPropertyRepository.GetByErrorId(errorId));

        return new ErrorDto
        {
            Error = error,
            Properties = properties
        };
    }

    public async Task<IReadOnlyCollection<Error>> GetErrors(Guid currentMemberId, int page, int pageSize)
    {
        return await GetSiteAdminRestrictedContent(currentMemberId,
            x => x.ErrorRepository.GetErrors(page, pageSize));
    }

    public bool IgnoreUnknownRequestPath(IHttpRequestContext httpRequestContext)
    {
        var path = UrlUtils.NormalisePath(httpRequestContext.RequestPath);
        var userAgent = httpRequestContext.UserAgent;

        return
            _settings.IgnoreUnknownPaths.Any(x => MatchesConfigRule(x, path)) ||
            _settings.IgnoreUnknownPathPatterns.Any(x => Regex.IsMatch(path, x)) ||
            _settings.IgnoreUnknownPathUserAgents.Any(x => MatchesConfigRule(x, userAgent));
    }

    public Task Info(string message)
        => Log(LogEventLevel.Information, message);

    public Task Warn(string message)
        => Log(LogEventLevel.Warning, message);

    public Task Warn(string message, IDictionary<string, string?> properties)
        => Log(LogEventLevel.Warning, message, properties);

    private Task Log(
        LogEventLevel level,
        string message,
        IDictionary<string, string?>? properties = null,
        Exception? exception = null)
    {
        var disposables = new List<IDisposable>();

        if (properties != null)
        {
            foreach (var key in properties.Keys)
            {
                disposables.Add(LogContext.PushProperty(key, properties[key]));
            }
        }

        switch (level)
        {
            case LogEventLevel.Information:
                _logger.Information(exception, message);
                break;

            case LogEventLevel.Warning:
                _logger.Warning(exception, message);
                break;

            case LogEventLevel.Error:
                _logger.Error(exception, message);
                break;
        }

        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }

        return Task.CompletedTask;
    }

    private bool MatchesConfigRule(string rule, string value)
    {
        var (wildStart, wildEnd) = (rule.StartsWith('*'), rule.EndsWith('*'));

        if (wildStart && wildEnd)
        {
            return value.Contains(rule[1..^1], StringComparison.OrdinalIgnoreCase);
        }

        if (wildStart)
        {
            return value.EndsWith(rule[1..], StringComparison.OrdinalIgnoreCase);
        }

        if (wildEnd)
        {
            return value.StartsWith(rule[..^1], StringComparison.OrdinalIgnoreCase);
        }

        return value.Equals(rule, StringComparison.OrdinalIgnoreCase);
    }
}