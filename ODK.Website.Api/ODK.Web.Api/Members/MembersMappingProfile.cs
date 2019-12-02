using AutoMapper;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Api.Members.Responses;

namespace ODK.Web.Api.Members
{
    public class MembersMappingProfile : Profile
    {
        public MembersMappingProfile()
        {
            CreateMap<Member, MemberApiResponse>();

            CreateMap<MemberProfile, MemberProfileApiResponse>()
                .ForMember(x => x.Properties, opt => opt.MapFrom(x => x.MemberProperties));

            CreateMap<MemberProperty, MemberProfilePropertyApiResponse>();
        }
    }
}
