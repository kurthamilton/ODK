using System.ComponentModel.DataAnnotations;

namespace ODK.Services.Users.ViewModels;

public class ChapterProfileFormSubmitViewModel : ProfileFormSubmitViewModel
{
    public List<ChapterProfileFormPropertyViewModel> Properties { get; set; } = new();

    [Required]
    public bool PrivacyPolicy { get; set; }
}
