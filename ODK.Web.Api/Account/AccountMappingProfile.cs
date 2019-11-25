using AutoMapper;
using ODK.Core.Members;
using ODK.Services.Authentication;
using ODK.Services.Members;
using ODK.Web.Api.Account.Responses;

namespace ODK.Web.Api.Account
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            CreateMap<MemberProfile, AccountProfileApiResponse>()
                .ForMember(x => x.Properties, opt => opt.MapFrom(x => x.MemberProperties));
            CreateMap<MemberProperty, AccountProfilePropertyApiResponse>();
            CreateMap<AuthenticationToken, AuthenticationTokenApiResponse>();
        }
    }
}
