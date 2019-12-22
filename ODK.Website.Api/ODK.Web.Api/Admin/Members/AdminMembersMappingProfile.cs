using AutoMapper;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Api.Admin.Members.Requests;
using ODK.Web.Api.Admin.Members.Responses;

namespace ODK.Web.Api.Admin.Members
{
    public class AdminMembersMappingProfile : Profile
    {
        public AdminMembersMappingProfile()
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
            CreateMap<MemberGroup, MemberGroupApiResponse>();

            CreateMap<MemberGroupMember, MemberGroupMemberApiResponse>();

            CreateMap<CreateMemberGroupApiRequest, CreateMemberGroup>();
        }
    }
}
