using AutoMapper;
using ODK.Core.Logging;
using ODK.Web.Api.Admin.Logging.Responses;

namespace ODK.Web.Api.Admin.Logging
{
    public class LoggingMappingProfile : Profile
    {
        public LoggingMappingProfile()
        {
            CreateMap<LogMessage, LogMessageApiResponse>();
        }
    }
}
