using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members
{
    public class AdminMemberModel : AdminPageModel
    {
        private readonly IChapterAdminService _chapterAdminService;

        public AdminMemberModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
            : base(requestCache)
        {
            _chapterAdminService = chapterAdminService;
        }

        public ChapterAdminMember AdminMember { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            AdminMember = await _chapterAdminService.GetChapterAdminMember(CurrentMemberId, Chapter.Id, id);
            if (AdminMember == null)
            {
                return Redirect($"/{Chapter.Name}/Admin/Members/AdminMembers");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, AdminMemberFormViewModel viewModel)
        {
            ServiceResult result = await _chapterAdminService.UpdateChapterAdminMember(CurrentMemberId, Chapter.Id, id, new UpdateChapterAdminMember
            {
                AdminEmailAddress = viewModel.AdminEmailAddress,
                ReceiveContactEmails = viewModel.ReceiveContactEmails,
                ReceiveNewMemberEmails = viewModel.ReceiveNewMemberEmails,
                SendNewMemberEmails = viewModel.SendNewMemberEmails
            });

            if (result.Success)
            {
                AddFeedback(new FeedbackViewModel("Chapter admin member updated", FeedbackType.Success));
                return Redirect($"/{Chapter.Name}/Admin/Members/AdminMembers");
            }

            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }
    }
}
