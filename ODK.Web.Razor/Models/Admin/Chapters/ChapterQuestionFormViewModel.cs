using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters
{
    public class ChapterQuestionFormViewModel
    {
        [Required]
        public string? Answer { get; set; }

        [Required]
        public string? Question { get; set; }
    }
}
