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
            CreateRequestMaps();
            CreateResponseMaps();
        }

        private void CreateRequestMaps()
        {
            CreateMap<UpdateMemberImageApiRequest, UpdateMemberImage>()
                .ForMember(x => x.MimeType, opt => opt.MapFrom(x => x.ContentType));

            CreateMap<UpdateMemberPropertyApiRequest, UpdateMemberProperty>();

            CreateMap<UpdateMemberProfileApiRequest, UpdateMemberProfile>();

            CreateMap<CreateMemberProfileApiRequest, CreateMemberProfile>()
                .IncludeBase<UpdateMemberProfileApiRequest, UpdateMemberProfile>()
                .ForMember(x => x.Image, opt => opt.Ignore());
        }

        private void CreateResponseMaps()
        {
            CreateMap<AuthenticationToken, AuthenticationTokenApiResponse>()
                .ForMember(x => x.AdminChapterIds, opt => opt.Condition(x => x.AdminChapterIds.Count > 0))
                .ForMember(x => x.SuperAdmin, opt => opt.Condition(x => x.SuperAdmin));

            CreateMap<MemberProfile, AccountProfileApiResponse>()
                .ForMember(x => x.Properties, opt => opt.MapFrom(x => x.MemberProperties));

            CreateMap<MemberProperty, AccountProfilePropertyApiResponse>();

            CreateMap<MemberSubscription, SubscriptionApiResponse>();
        }
    }
}
