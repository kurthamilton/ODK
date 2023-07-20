using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters
{
    public class ChapterEmailFormViewModel
    {
        [Required]
        public string Content { get; set; } = "";

        [Required]
        public string Subject { get; set; } = "";
    }
}
