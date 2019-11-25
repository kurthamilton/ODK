using AutoMapper;
using ODK.Core.Members;
using ODK.Services.Authentication;
using ODK.Services.Members;

namespace ODK.Web.Api.Account
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            CreateMap<MemberProfile, AccountProfileResponse>()
                .ForMember(x => x.Properties, opt => opt.MapFrom(x => x.MemberProperties));
            CreateMap<MemberProperty, AccountProfilePropertyResponse>();
            CreateMap<AuthenticationToken, AuthenticationTokenResponse>();
        }
    }
}
