using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Logging;
using ODK.Services.Exceptions;
using Serilog;

namespace ODK.Services.Logging
{
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

            LogMessage logMessage = await _loggingRepository.GetLogMessage(logMessageId);
            if (logMessage == null)
            {
                return;
            }

            await _loggingRepository.DeleteLogMessages(logMessage.Message);
        }

        public async Task<LogMessage> GetErrorMessage(Guid currentMemberId, int logMessageId)
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

        public Task LogError(Exception exception, string message)
        {
            _logger.Error(exception, message);

            return Task.CompletedTask;
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
}
