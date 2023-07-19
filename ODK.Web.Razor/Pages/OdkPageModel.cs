using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages
{
    public abstract class OdkPageModel : PageModel
    {
        protected OdkPageModel(IRequestCache requestCache)
        {
            RequestCache = requestCache;
        }

        public Member? CurrentMember { get; private set; }
        
        public IRequestCache RequestCache { get; private set; }

        public string? Title
        {
            get => ViewData["Title"] as string;
            set => ViewData["Title"] = value;
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            Guid? memberId = HttpContext.User.MemberId();
            CurrentMember = memberId.HasValue ? await RequestCache.GetMember(memberId.Value) : null;

            await base.OnPageHandlerExecutionAsync(context, next);
        }

        protected void AddFeedback(FeedbackViewModel viewModel)
        {
            TempData.AddFeedback(viewModel);
        }
    }
}
