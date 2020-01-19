using AutoMapper;
using ODK.Services.Members;
using ODK.Web.Api.Admin.Members.Requests;

namespace ODK.Web.Api.Admin.Members
{
    public class MembersMappingProfile : Profile
    {
        public MembersMappingProfile()
        {
            MapRequests();
        }

        private void MapRequests()
        {
            CreateMap<UpdateMemberSubscriptionApiRequest, UpdateMemberSubscription>();
        }
    }
}
