﻿using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members
{
    public abstract class MemberAdminPageModel : AdminPageModel
    {
        protected MemberAdminPageModel(IRequestCache requestCache, IMemberAdminService memberAdminService) 
            : base(requestCache)
        {
            MemberAdminService = memberAdminService;
        }

        public Member Member { get; private set; } = null!;

        protected IMemberAdminService MemberAdminService { get; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id);
            Member = await MemberAdminService.GetMember(CurrentMemberId, id);
            if (Member == null)
            {
                Response.Redirect($"{Request.RouteValues["chapterName"]}/Admin/Members");
                return;
            }

            await base.OnPageHandlerExecutionAsync(context, next);
        }
    }
}