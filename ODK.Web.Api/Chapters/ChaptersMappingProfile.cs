using System;
using AutoMapper;
using ODK.Core.Chapters;

namespace ODK.Web.Api.Chapters
{
    public class ChaptersMappingProfile : Profile
    {
        public ChaptersMappingProfile()
        {
            CreateMap<Chapter, ChapterResponse>();

            CreateMap<Chapter, ChapterDetailsResponse>();

            CreateMap<ChapterLinks, ChapterLinksResponse>()
                .ForMember(x => x.Facebook, opt => opt.MapFrom(x => x.FacebookName))
                .ForMember(x => x.Instagram, opt => opt.MapFrom(x => x.InstagramName))
                .ForMember(x => x.Twitter, opt => opt.MapFrom(x => x.TwitterName));

            CreateMap<ChapterProperty, ChapterPropertyResponse>()
                .ForMember(x => x.Required, opt => opt.Condition(x => x.Required));

            CreateMap<ChapterPropertyOption, ChapterPropertyOptionResponse>()
                .ForMember(x => x.FreeText, opt => opt.MapFrom(x => x.Value.Equals("Other", StringComparison.OrdinalIgnoreCase) ? true : new bool?()));
        }
    }
}
