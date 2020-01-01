using AutoMapper;
using ODK.Core.Logging;
using ODK.Web.Api.Admin.Admin.Responses;

namespace ODK.Web.Api.Admin.Admin
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            CreateMap<LogMessage, LogMessageApiResponse>();
        }
    }
}
