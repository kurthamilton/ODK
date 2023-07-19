using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DataType = ODK.Core.DataTypes.DataType;

namespace ODK.Web.Razor.Models.Admin.Chapters
{
    public class ChapterPropertyFormViewModel
    {
        [Required]
        public DataType DataType { get; set; }

        [DisplayName("Help text")]
        public string? HelpText { get; set; }

        public bool Hidden { get; set; }

        [Required]
        public string? Label { get; set; }

        [Required]
        public string? Name { get; set; }

        public bool Required { get; set; }

        public string? Subtitle { get; set; }
    }
}
