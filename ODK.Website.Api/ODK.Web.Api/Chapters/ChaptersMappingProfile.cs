using System;
using AutoMapper;
using ODK.Core.Chapters;
using ODK.Web.Api.Chapters.Responses;

namespace ODK.Web.Api.Chapters
{
    public class ChaptersMappingProfile : Profile
    {
        public ChaptersMappingProfile()
        {
            CreateResponseMaps();
        }

        private void CreateResponseMaps()
        {
            CreateMap<Chapter, ChapterApiResponse>();

            CreateMap<ChapterLinks, ChapterLinksApiResponse>()
                .ForMember(x => x.Facebook, opt => opt.MapFrom(x => x.FacebookName))
                .ForMember(x => x.Instagram, opt => opt.MapFrom(x => x.InstagramName))
                .ForMember(x => x.Twitter, opt => opt.MapFrom(x => x.TwitterName));

            CreateMap<ChapterPaymentSettings, ChapterPaymentSettingsApiResponse>();

            CreateMap<ChapterProperty, ChapterPropertyApiResponse>()
                .ForMember(x => x.DataTypeId, opt => opt.MapFrom(x => x.DataType))
                .ForMember(x => x.Required, opt => opt.Condition(x => x.Required));

            CreateMap<ChapterPropertyOption, ChapterPropertyOptionApiResponse>()
                .ForMember(x => x.FreeText, opt => opt.MapFrom(x => x.Value.Equals("Other", StringComparison.OrdinalIgnoreCase) ? true : new bool?()));

            CreateMap<ChapterQuestion, ChapterQuestionApiResponse>();

            CreateMap<ChapterSubscription, ChapterSubscriptionApiResponse>();

            CreateMap<ChapterTexts, ChapterTextsApiResponse>();
        }
    }
}
