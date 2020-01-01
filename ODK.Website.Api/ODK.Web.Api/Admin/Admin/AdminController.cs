using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Logging;
using ODK.Services.Logging;
using ODK.Web.Api.Admin.Admin.Responses;

namespace ODK.Web.Api.Admin.Admin
{
    [Authorize]
    [ApiController]
    [Route("Admin")]
    public class AdminController : OdkControllerBase
    {
        private readonly ILoggingService _loggingService;
        private readonly IMapper _mapper;

        public AdminController(ILoggingService loggingService, IMapper mapper)
        {
            _loggingService = loggingService;
            _mapper = mapper;
        }

        [HttpGet("Log")]
        public async Task<IEnumerable<LogMessageApiResponse>> GetLogMessages(string level, int? page)
        {
            IReadOnlyCollection<LogMessage> logMessages = await _loggingService.GetLogMessages(GetMemberId(), level, page ?? 1, 200);
            return logMessages.Select(_mapper.Map<LogMessageApiResponse>);
        }
    }
}
