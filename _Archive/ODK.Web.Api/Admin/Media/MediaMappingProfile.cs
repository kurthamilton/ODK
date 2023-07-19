using AutoMapper;
using ODK.Core.Media;
using ODK.Web.Api.Admin.Media.Responses;

namespace ODK.Web.Api.Admin.Media
{
    public class MediaMappingProfile : Profile
    {
        public MediaMappingProfile()
        {
            MapResponses();
        }

        private void MapResponses()
        {
            CreateMap<MediaFile, MediaFileApiResponse>();
        }
    }
}
