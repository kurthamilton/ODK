using AutoMapper;
using ODK.Core.Members;
using ODK.Services.Members;

namespace ODK.Web.Api.Members
{
    public class MembersMappingProfile : Profile
    {
        public MembersMappingProfile()
        {
            CreateMap<Member, MemberResponse>();

            CreateMap<MemberProfile, MemberProfileResponse>()
                .ForMember(x => x.Properties, opt => opt.MapFrom(x => x.MemberProperties));

            CreateMap<MemberProperty, MemberProfilePropertyResponse>();
        }
    }
}
