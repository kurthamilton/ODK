using System;
using AutoMapper;
using ODK.Core.Chapters;
using ODK.Core.Mail;
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

            CreateMap<UpdateChapterAdminMemberApiRequest, UpdateChapterAdminMember>();

            CreateMap<UpdateChapterEmailApiRequest, UpdateChapterEmail>();

            CreateMap<UpdateChapterEmailProviderSettingsApiRequest, UpdateChapterEmailProviderSettings>();

            CreateMap<UpdateChapterLinksApiRequest, UpdateChapterLinks>();

            CreateMap<UpdateChapterPaymentSettingsApiRequest, UpdateChapterPaymentSettings>();

            CreateMap<UpdateChapterTextsApiRequest, UpdateChapterTexts>();
        }

        private void MapResponses()
        {
            CreateMap<ChapterAdminMember, ChapterAdminMemberApiResponse>();

            CreateMap<ChapterEmail, ChapterEmailApiResponse>()
                .ForMember(x => x.Id, opt => opt.Condition(x => x.Id != Guid.Empty));

            CreateMap<ChapterEmailProviderSettings, ChapterEmailProviderSettingsApiResponse>();

            CreateMap<ChapterPaymentSettings, ChapterAdminPaymentSettingsApiResponse>();
        }
    }
}
