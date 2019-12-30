using AutoMapper;
using ODK.Services.SocialMedia;
using ODK.Web.Api.SocialMedia.Responses;

namespace ODK.Web.Api.SocialMedia
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
