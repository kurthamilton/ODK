using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters
{
    public class PropertyCreateModel : AdminPageModel
    {
        private readonly IChapterAdminService _chapterAdminService;

        public PropertyCreateModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
            : base(requestCache)
        {
            _chapterAdminService = chapterAdminService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(ChapterPropertyFormViewModel viewModel)
        {
            ServiceResult result = await _chapterAdminService.CreateChapterProperty(CurrentMemberId, Chapter.Id, new CreateChapterProperty
            {
                DataType = viewModel.DataType,
                HelpText = viewModel.HelpText,
                Hidden = viewModel.Hidden,
                Label = viewModel.Label,
                Name = viewModel.Name,
                Required = viewModel.Required,
                Subtitle = viewModel.Subtitle
            });

            if (!result.Success)
            {
                AddFeedback(new FeedbackViewModel(result));
                return Page();
            }

            AddFeedback(new FeedbackViewModel("Property created", FeedbackType.Success));
            return Redirect($"/{Chapter.Name}/Admin/Chapter/Properties");
        }
    }
}
