using AutoMapper;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Common.Members.Responses;

namespace ODK.Web.Common.Members
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
