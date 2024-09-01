using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DataType = ODK.Core.DataTypes.DataType;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterPropertyFormViewModel
{
    [DisplayName("Application form only")]
    public bool ApplicationOnly { get; set; }

    [Required]
    public DataType DataType { get; set; }

    public bool DataTypeEnabled { get; set; }

    [DisplayName("Display name")]
    public string? DisplayName { get; set; }

    [DisplayName("Help text")]
    public string? HelpText { get; set; }

    [Required]
    public string Label { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    public string Options { get; set; } = "";

    public bool Required { get; set; }

    public string? Subtitle { get; set; }
}
