using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters
{
    public class PropertyEditModel : AdminPageModel
    {
        private readonly IChapterAdminService _chapterAdminService;

        public PropertyEditModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
            : base(requestCache)
        {
            _chapterAdminService = chapterAdminService;
        }

        public ChapterProperty Property { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Property = await _chapterAdminService.GetChapterProperty(CurrentMemberId, id);
            if (Property == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, ChapterPropertyFormViewModel viewModel)
        {
            ServiceResult result = await _chapterAdminService.UpdateChapterProperty(CurrentMemberId, id, new UpdateChapterProperty
            {
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

            AddFeedback(new FeedbackViewModel("Property updated", FeedbackType.Success));
            return Redirect($"/{Chapter.Name}/Admin/Chapter/Properties");
        }
    }
}
