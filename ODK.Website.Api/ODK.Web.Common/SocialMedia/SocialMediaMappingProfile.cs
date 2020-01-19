using AutoMapper;
using ODK.Services.SocialMedia;
using ODK.Web.Common.SocialMedia.Responses;

namespace ODK.Web.Common.SocialMedia
{
    public class SocialMediaMappingProfile : Profile
    {
        public SocialMediaMappingProfile()
        {
            CreateResponseMaps();
        }

        private void CreateResponseMaps()
        {
            CreateMap<SocialMediaImage, SocialMediaImageApiResponse>();
        }
    }
}
