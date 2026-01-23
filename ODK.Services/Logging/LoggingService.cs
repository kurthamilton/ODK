using ODK.Core.Logging;
using ODK.Data.Core;
using Serilog;
using Serilog.Context;

namespace ODK.Services.Logging;

public class LoggingService : OdkAdminServiceBase, ILoggingService
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public LoggingService(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory, IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _logger = logger;
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
    {
        _logger.Error(message);

        return Task.CompletedTask;
    }

    public Task Error(string message, Exception exception)
    {
        _logger.Error(exception, message);

        return Task.CompletedTask;
    }

    public async Task Error(Exception exception, HttpRequest request)
    {
        _logger.Error(exception, exception.Message);

        // Create new unit of work to avoid re-instigating any previous context errors
        var unitOfWork = _unitOfWorkFactory.Create();

        var error = Core.Logging.Error.FromException(exception);
        unitOfWork.ErrorRepository.Add(error);

        var properties = new List<ErrorProperty>
        {
            new ErrorProperty
            {
                ErrorId = error.Id,
                Name = "REQUEST.URL",
                Value = request.Url
            },
            new ErrorProperty
            {
                ErrorId = error.Id,
                Name = "REQUEST.METHOD",
                Value = request.Method
            },
            new ErrorProperty
            {
                ErrorId = error.Id,
                Name = "REQUEST.USERNAME",
                Value = request.Username ?? string.Empty
            }
        };

        foreach (string key in request.Headers.Keys)
        {
            properties.Add(new ErrorProperty
            {
                ErrorId = error.Id,
                Name = $"REQUEST.HEADER.{key.ToUpperInvariant()}",
                Value = request.Headers[key]
            });
        }

        foreach (string key in request.Form.Keys)
        {
            properties.Add(new ErrorProperty
            {
                ErrorId = error.Id,
                Name = $"REQUEST.FORM.{key.ToUpperInvariant()}",
                Value = request.Form[key]
            });
        }

        properties.Add(new ErrorProperty
        {
            ErrorId = error.Id,
            Name = "EXCEPTION.STACKTRACE",
            Value = exception.StackTrace?.Replace(Environment.NewLine, "<br>") ?? string.Empty
        });

        var innerException = exception.InnerException;
        var innerExceptionCount = 0;
        while (innerException != null)
        {
            properties.Add(new ErrorProperty
            {
                ErrorId = error.Id,
                Name = $"EXCEPTION.INNEREXCEPTION[{innerExceptionCount}].TYPE",
                Value = innerException.GetType().Name
            });
            properties.Add(new ErrorProperty
            {
                ErrorId = error.Id,
                Name = $"EXCEPTION.INNEREXCEPTION[{innerExceptionCount}].MESSAGE",
                Value = innerException.Message
            });

            innerException = innerException.InnerException;
            innerExceptionCount++;
        }

        unitOfWork.ErrorPropertyRepository.AddMany(properties);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task Error(Exception exception, IDictionary<string, string> data)
    {
        _logger.Error(exception, exception.Message);

        // Create new unit of work to avoid re-instigating any previous context errors
        var unitOfWork = _unitOfWorkFactory.Create();

        var error = Core.Logging.Error.FromException(exception);
        unitOfWork.ErrorRepository.Add(error);

        var properties = data
            .Select(x => new ErrorProperty
            {
                ErrorId = error.Id,
                Name = x.Key,
                Value = x.Value
            })
            .ToArray();

        unitOfWork.ErrorPropertyRepository.AddMany(properties);

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

    public Task Info(string message)
    {
        _logger.Information(message);
        return Task.CompletedTask;
    }

    public Task Warn(string message) => Warn(message, new Dictionary<string, string?>());

    public Task Warn(string message, IDictionary<string, string?> properties)
    {
        var disposables = new List<IDisposable>();

        foreach (var key in properties.Keys)
        {
            disposables.Add(LogContext.PushProperty(key, properties[key]));
        }

        _logger.Warning(message);

        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }

        return Task.CompletedTask;
    }
}
