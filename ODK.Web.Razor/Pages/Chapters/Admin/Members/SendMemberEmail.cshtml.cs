using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class SendMemberEmailModel : AdminPageModel
{
    public SendMemberEmailModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public Guid MemberId { get; private set; }

    public void OnGet(Guid id)
    {
        MemberId = id;
    }
}
