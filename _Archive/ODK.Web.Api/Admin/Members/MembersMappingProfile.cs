using AutoMapper;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Api.Admin.Members.Requests;
using ODK.Web.Api.Admin.Members.Responses;
using ODK.Web.Common.Members.Responses;

namespace ODK.Web.Api.Admin.Members
{
    public class MembersMappingProfile : Profile
    {
        public MembersMappingProfile()
        {
            MapRequests();
            MapResponses();
        }

        private void MapRequests()
        {
            CreateMap<UpdateMemberSubscriptionApiRequest, UpdateMemberSubscription>();
        }

        private void MapResponses()
        {
            CreateMap<Member, MemberAdminApiResponse>()
                .IncludeBase<Member, MemberApiResponse>();

            CreateMap<Member, MemberEmailApiResponse>()
                .ForMember(x => x.MemberId, opt => opt.MapFrom(x => x.Id));
        }
    }
}
