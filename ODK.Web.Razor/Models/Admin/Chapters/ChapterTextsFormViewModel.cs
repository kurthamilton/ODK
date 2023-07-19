using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters
{
    public class ChapterTextsFormViewModel
    {
        public Guid ChapterId { get; set; }

        [Required]
        [DisplayName("Register message")]
        public string? RegisterMessage { get; set; }

        [Required]
        [DisplayName("Welcome message")]
        public string? WelcomeMessage { get; set; }
    }
}
