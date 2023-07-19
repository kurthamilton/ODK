using AutoMapper;
using ODK.Core.Chapters;
using ODK.Services.Chapters;
using ODK.Web.Api.Admin.Chapters.Requests;
using ODK.Web.Api.Admin.Chapters.Responses;

namespace ODK.Web.Api.Admin.Chapters
{
    public class ChaptersMappingProfile : Profile
    {
        public ChaptersMappingProfile()
        {
            MapRequests();
            MapResponses();
        }

        private void MapRequests()
        {
            CreateMap<CreateChapterPropertyApiRequest, CreateChapterProperty>()
                .IncludeBase<UpdateChapterPropertyApiRequest, UpdateChapterProperty>();

            CreateMap<CreateChapterQuestionApiRequest, CreateChapterQuestion>();

            CreateMap<CreateChapterSubscriptionApiRequest, CreateChapterSubscription>();

            CreateMap<UpdateChapterAdminMemberApiRequest, UpdateChapterAdminMember>();

            CreateMap<UpdateChapterLinksApiRequest, UpdateChapterLinks>();

            CreateMap<UpdateChapterMembershipSettingsApiRequest, UpdateChapterMembershipSettings>();

            CreateMap<UpdateChapterPaymentSettingsApiRequest, UpdateChapterPaymentSettings>();

            CreateMap<UpdateChapterPropertyApiRequest, UpdateChapterProperty>();

            CreateMap<UpdateChapterTextsApiRequest, UpdateChapterTexts>();
        }

        private void MapResponses()
        {
            CreateMap<ChapterAdminMember, ChapterAdminMemberApiResponse>();

            CreateMap<ChapterMembershipSettings, ChapterAdminMembershipSettingsApiResponse>();

            CreateMap<ChapterPaymentSettings, ChapterAdminPaymentSettingsApiResponse>();
        }
    }
}
