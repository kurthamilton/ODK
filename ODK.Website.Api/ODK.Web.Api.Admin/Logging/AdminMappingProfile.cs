using AutoMapper;
using ODK.Core.Logging;
using ODK.Web.Api.Admin.Logging.Responses;

namespace ODK.Web.Api.Admin.Logging
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            CreateMap<LogMessage, LogMessageApiResponse>();
        }
    }
}
