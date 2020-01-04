using AutoMapper;
using ODK.Services.Members;
using ODK.Web.Api.Admin.Members.Requests;

namespace ODK.Web.Api.Admin.Members
{
    public class AdminMembersMappingProfile : Profile
    {
        public AdminMembersMappingProfile()
        {
            MapRequests();
        }

        private void MapRequests()
        {
            CreateMap<UpdateMemberSubscriptionApiRequest, UpdateMemberSubscription>();
        }
    }
}
