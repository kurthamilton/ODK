using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Chapters;

public class CreateChapterSubmitViewModel
{
    [DisplayName("Country")]
    [Required]
    public Guid? CountryId { get; set; }

    [Required]
    [MaxLength(1024)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [DisplayName("Timezone")]
    public string? TimeZoneId { get; set; }
}
