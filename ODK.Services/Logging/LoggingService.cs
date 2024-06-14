using ODK.Core.Chapters;
using ODK.Core.Logging;
using ODK.Services.Exceptions;
using Serilog;

namespace ODK.Services.Logging;

public class LoggingService : ILoggingService
{
    private readonly IChapterRepository _chapterRepository;
    private readonly ILogger _logger;
    private readonly ILoggingRepository _loggingRepository;

    public LoggingService(ILogger logger, ILoggingRepository loggingRepository,
        IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
        _logger = logger;
        _loggingRepository = loggingRepository;
    }

    public async Task DeleteError(Guid currentMemberId, int logMessageId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId);

        await _loggingRepository.DeleteLogMessage(logMessageId);
    }

    public async Task DeleteAllErrors(Guid currentMemberId, int logMessageId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId);

        LogMessage? logMessage = await _loggingRepository.GetLogMessage(logMessageId);
        if (logMessage == null)
        {
            return;
        }

        await _loggingRepository.DeleteLogMessages(logMessage.Message);
    }

    public async Task<LogMessage?> GetErrorMessage(Guid currentMemberId, int logMessageId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId);

        return await _loggingRepository.GetLogMessage(logMessageId);
    }

    public async Task<IReadOnlyCollection<LogMessage>> GetErrorMessages(Guid currentMemberId)
    {
        return await GetLogMessages(currentMemberId, "Error", 1, 0);
    }

    public async Task<IReadOnlyCollection<LogMessage>> GetLogMessages(Guid currentMemberId, string level, int page, int pageSize)
    {
        await AssertMemberIsChapterAdmin(currentMemberId);

        return await _loggingRepository.GetLogMessages(level, page, pageSize);
    }

    public async Task<IReadOnlyCollection<LogMessage>> GetSimilarErrorMessages(Guid currentMemberId,
        LogMessage logMessage)
    {
        await AssertMemberIsChapterAdmin(currentMemberId);

        IReadOnlyCollection<LogMessage> messages = await _loggingRepository.GetLogMessages("Error", 1, 0, logMessage.Message);

        return messages
            .Where(x => x.Id != logMessage.Id)
            .ToArray();
    }

    public Task LogDebug(string message)
    {
        _logger.Information(message);

        return Task.CompletedTask;
    }

    public async Task LogError(Exception exception, HttpRequest request)
    {
        Error error = new Error(Guid.NewGuid(), DateTime.UtcNow, exception.GetType().Name, exception.Message);

        List<ErrorProperty> properties = new List<ErrorProperty>
        {
            new ErrorProperty(error.Id, "REQUEST.URL", request.Url),
            new ErrorProperty(error.Id, "REQUEST.METHOD", request.Method),
            new ErrorProperty(error.Id, "REQUEST.USERNAME", request.Username ?? "")
        };

        foreach (string key in request.Headers.Keys)
        {
            properties.Add(new ErrorProperty(error.Id, $"REQUEST.HEADER.{key.ToUpperInvariant()}", request.Headers[key]));
        }

        foreach (string key in request.Form.Keys)
        {
            properties.Add(new ErrorProperty(error.Id, $"REQUEST.FORM.{key.ToUpperInvariant()}", request.Form[key]));
        }

        properties.Add(new ErrorProperty(error.Id, "EXCEPTION.STACKTRACE", 
            exception.StackTrace.Replace(Environment.NewLine, "<br>")));

        Exception? innerException = exception.InnerException;
        int innerExceptionCount = 0;
        while (innerException != null)
        {
            properties.Add(new ErrorProperty(error.Id, $"EXCEPTION.INNEREXCEPTION[{innerExceptionCount}].TYPE", innerException.GetType().Name));
            properties.Add(new ErrorProperty(error.Id, $"EXCEPTION.INNEREXCEPTION[{innerExceptionCount}].MESSAGE", innerException.Message));
            
            innerException = innerException.InnerException;
            innerExceptionCount++;
        }

        await _loggingRepository.LogError(error, properties);
    }

    public async Task LogError(Exception exception, IDictionary<string, string> data)
    {
        Error error = new Error(Guid.NewGuid(), DateTime.UtcNow, exception.GetType().Name, exception.Message);

        IReadOnlyCollection<ErrorProperty> properties = data
            .Select(x => new ErrorProperty(error.Id, x.Key, x.Value))
            .ToArray();

        await _loggingRepository.LogError(error, properties);
    }

    private async Task AssertMemberIsChapterAdmin(Guid currentMemberId)
    {
        IReadOnlyCollection<ChapterAdminMember> adminMembers = await _chapterRepository.GetChapterAdminMembersByMember(currentMemberId);
        if (!adminMembers.Any(x => x.SuperAdmin))
        {
            throw new OdkNotAuthorizedException();
        }
    }
}
