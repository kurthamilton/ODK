using ODK.Core.Logging;
using ODK.Data.Core;
using Serilog;

namespace ODK.Services.Logging;

public class LoggingService : OdkAdminServiceBase, ILoggingService
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LoggingService(ILogger logger, IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task DeleteError(Guid currentMemberId, int logMessageId)
    {        
        var currentMember = await _unitOfWork.MemberRepository.GetById(currentMemberId).RunAsync();
        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository.GetByChapterId(currentMember.ChapterId).RunAsync();
        AssertMemberIsChapterAdmin(currentMemberId, currentMember.ChapterId, chapterAdminMembers);

        // await _loggingRepository.DeleteLogMessageAsync(logMessageId);
    }

    public async Task DeleteAllErrors(Guid currentMemberId, int logMessageId)
    {
        var currentMember = await _unitOfWork.MemberRepository.GetById(currentMemberId).RunAsync();
        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository.GetByChapterId(currentMember.ChapterId).RunAsync();
        AssertMemberIsChapterAdmin(currentMemberId, currentMember.ChapterId, chapterAdminMembers);

        // LogMessage? logMessage = await _loggingRepository.GetLogMessageAsync(logMessageId);
        // if (logMessage == null)
        // {
        //     return;
        // }
        // 
        // await _loggingRepository.DeleteLogMessagesAsync(logMessage.Message);
    }

    public Task LogDebug(string message)
    {
        _logger.Information(message);

        return Task.CompletedTask;
    }

    public async Task LogError(Exception exception, HttpRequest request)
    {
        var error = Error.FromException(exception);
        _unitOfWork.ErrorRepository.Add(error);

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
                Value = request.Username ?? ""
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
            Value = exception.StackTrace?.Replace(Environment.NewLine, "<br>") ?? ""
        });

        Exception? innerException = exception.InnerException;
        int innerExceptionCount = 0;
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
        
        _unitOfWork.ErrorPropertyRepository.AddMany(properties);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task LogError(Exception exception, IDictionary<string, string> data)
    {
        var error = Error.FromException(exception);
        _unitOfWork.ErrorRepository.Add(error);

        var properties = data
            .Select(x => new ErrorProperty
            {
                ErrorId = error.Id,
                Name = x.Key,
                Value = x.Value
            })
            .ToArray();

        _unitOfWork.ErrorPropertyRepository.AddMany(properties);

        await _unitOfWork.SaveChangesAsync();
    }
}
