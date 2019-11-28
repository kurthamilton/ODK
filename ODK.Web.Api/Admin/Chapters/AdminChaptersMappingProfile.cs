using AutoMapper;
using ODK.Services.Chapters;
using ODK.Web.Api.Admin.Chapters.Requests;

namespace ODK.Web.Api.Admin.Chapters
{
    public class AdminChaptersMappingProfile : Profile
    {
        public AdminChaptersMappingProfile()
        {
            MapRequests();
        }

        private void MapRequests()
        {
            CreateMap<UpdateChapterLinksApiRequest, UpdateChapterLinks>();
        }
    }
}
