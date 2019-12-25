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

            CreateMap<UpdateChapterEmailProviderSettingsApiRequest, UpdateChapterEmailProviderSettings>();

            CreateMap<UpdateChapterEmailSettingsApiRequest, UpdateChapterEmailSettings>();

            CreateMap<UpdateChapterLinksApiRequest, UpdateChapterLinks>();

            CreateMap<UpdateChapterPaymentSettingsApiRequest, UpdateChapterPaymentSettings>();

            CreateMap<UpdateChapterTextsApiRequest, UpdateChapterTexts>();
        }

        private void MapResponses()
        {
            CreateMap<ChapterEmailSettings, ChapterEmailSettingsApiResponse>();

            CreateMap<ChapterEmailProviderSettings, ChapterEmailProviderSettingsApiResponse>();

            CreateMap<ChapterPaymentSettings, ChapterAdminPaymentSettingsApiResponse>();
        }
    }
}
