using Microsoft.AspNetCore.Authorization;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Razor.Pages.Chapters.Admin
{
    [Authorize(Roles = "Admin")]
    public abstract class AdminPageModel : ChapterPageModel
    {
        protected AdminPageModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public new Member CurrentMember => base.CurrentMember!;

        protected Guid CurrentMemberId => User.MemberId()!.Value;
    }
}
