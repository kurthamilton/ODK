using AutoMapper;
using ODK.Core.Members;
using ODK.Services.Authentication;
using ODK.Services.Members;
using ODK.Web.Api.Account.Requests;
using ODK.Web.Api.Account.Responses;

namespace ODK.Web.Api.Account
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            MapRequests();
            MapResponses();
        }

        private void MapRequests()
        {
            CreateMap<UpdateMemberPropertyApiRequest, UpdateMemberProperty>();

            CreateMap<UpdateMemberProfileApiRequest, UpdateMemberProfile>();

            CreateMap<CreateMemberProfileApiRequest, CreateMemberProfile>()
                .IncludeBase<UpdateMemberProfileApiRequest, UpdateMemberProfile>();
        }

        private void MapResponses()
        {
            CreateMap<MemberProfile, AccountProfileApiResponse>()
                .ForMember(x => x.Properties, opt => opt.MapFrom(x => x.MemberProperties));

            CreateMap<MemberProperty, AccountProfilePropertyApiResponse>();

            CreateMap<AuthenticationToken, AuthenticationTokenApiResponse>();
        }
    }
}
