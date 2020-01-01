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

        public async Task<IReadOnlyCollection<LogMessage>> GetLogMessages(Guid currentMemberId, string level, int page, int pageSize)
        {
            IReadOnlyCollection<ChapterAdminMember> adminMembers = await _chapterRepository.GetChapterAdminMembersByMember(currentMemberId);
            if (!adminMembers.Any(x => x.SuperAdmin))
            {
                throw new OdkNotAuthorizedException();
            }

            return await _loggingRepository.GetLogMessages(level, page, pageSize);
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
    }
}
