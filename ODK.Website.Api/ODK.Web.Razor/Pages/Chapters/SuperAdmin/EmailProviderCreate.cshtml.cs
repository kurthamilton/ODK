using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin
{
    public class EmailProviderCreateModel : SuperAdminPageModel
    {
        private readonly IEmailAdminService _emailAdminService;

        public EmailProviderCreateModel(IRequestCache requestCache, IEmailAdminService emailAdminService) 
            : base(requestCache)
        {
            _emailAdminService = emailAdminService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(EmailProviderFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            ServiceResult result = await _emailAdminService.AddChapterEmailProvider(CurrentMemberId, Chapter.Id, new UpdateChapterEmailProvider
            {
                BatchSize = viewModel.BatchSize,
                DailyLimit = viewModel.DailyLimit ?? 0,
                FromEmailAddress = viewModel.FromEmailAddress,
                FromName = viewModel.FromName,
                SmtpLogin = viewModel.SmtpLogin,
                SmtpPassword = viewModel.SmtpPassword,
                SmtpPort = viewModel.SmtpPort ?? 0,
                SmtpServer = viewModel.SmtpServer
            });

            if (!result.Success)
            {
                AddFeedback(new FeedbackViewModel(result));
                return Page();
            }

            AddFeedback(new FeedbackViewModel("Email provider created", FeedbackType.Success));
            return Redirect($"/{Chapter.Name}/Admin/SuperAdmin/EmailProviders");
        }
    }
}
