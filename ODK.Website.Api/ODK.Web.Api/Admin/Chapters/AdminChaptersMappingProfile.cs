using AutoMapper;
using ODK.Core.Chapters;
using ODK.Services.Chapters;
using ODK.Web.Api.Admin.Chapters.Requests;
using ODK.Web.Api.Admin.Chapters.Responses;

namespace ODK.Web.Api.Admin.Chapters
{
    public class AdminChaptersMappingProfile : Profile
    {
        public AdminChaptersMappingProfile()
        {
            MapRequests();
            MapResponses();
        }

        private void MapRequests()
        {
            CreateMap<CreateChapterQuestionApiRequest, CreateChapterQuestion>();

            CreateMap<UpdateChapterDetailsApiRequest, UpdateChapterDetails>();

            CreateMap<UpdateChapterEmailSettingsApiRequest, UpdateChapterEmailSettings>();

            CreateMap<UpdateChapterLinksApiRequest, UpdateChapterLinks>();

            CreateMap<UpdateChapterPaymentSettingsApiRequest, UpdateChapterPaymentSettings>();
        }

        private void MapResponses()
        {
            CreateMap<ChapterEmailSettings, ChapterEmailSettingsApiResponse>();

            CreateMap<ChapterPaymentSettings, ChapterAdminPaymentSettingsApiResponse>();
        }
    }
}
