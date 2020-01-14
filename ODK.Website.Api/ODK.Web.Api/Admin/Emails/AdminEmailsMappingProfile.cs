using System;
using AutoMapper;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Web.Api.Admin.Emails.Requests;
using ODK.Web.Api.Admin.Emails.Responses;

namespace ODK.Web.Api.Admin.Emails
{
    public class AdminEmailsMappingProfile : Profile
    {
        public AdminEmailsMappingProfile()
        {
            MapRequests();
            MapResponses();
        }

        private void MapRequests()
        {
            CreateMap<UpdateEmailApiRequest, UpdateEmail>();

            CreateMap<UpdateChapterEmailProviderApiRequest, UpdateChapterEmailProvider>();
        }

        private void MapResponses()
        {
            CreateMap<ChapterEmail, ChapterEmailApiResponse>()
                .ForMember(x => x.Id, opt => opt.Condition(x => x.Id != Guid.Empty));

            CreateMap<ChapterEmailProvider, ChapterEmailProviderApiResponse>();

            CreateMap<Email, EmailApiResponse>()
                .ForMember(x => x.HtmlContent, opt => opt.MapFrom(x => x.HtmlContent));
        }
    }
}
