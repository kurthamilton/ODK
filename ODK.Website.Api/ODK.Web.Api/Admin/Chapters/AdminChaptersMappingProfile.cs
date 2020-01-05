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

            CreateMap<CreateChapterSubscriptionApiRequest, CreateChapterSubscription>();

            CreateMap<UpdateChapterAdminMemberApiRequest, UpdateChapterAdminMember>();

            CreateMap<UpdateChapterLinksApiRequest, UpdateChapterLinks>();

            CreateMap<UpdateChapterPaymentSettingsApiRequest, UpdateChapterPaymentSettings>();

            CreateMap<UpdateChapterTextsApiRequest, UpdateChapterTexts>();
        }

        private void MapResponses()
        {
            CreateMap<ChapterAdminMember, ChapterAdminMemberApiResponse>();

            CreateMap<ChapterPaymentSettings, ChapterAdminPaymentSettingsApiResponse>();
        }
    }
}
