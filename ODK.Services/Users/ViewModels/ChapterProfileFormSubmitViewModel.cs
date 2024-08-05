using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Services.Users.ViewModels;

public class ChapterProfileFormSubmitViewModel : ProfileFormSubmitViewModel
{

    [DisplayName("Receive event emails")]
    public bool EmailOptIn { get; set; } = true;


    public List<ChapterProfileFormPropertyViewModel> Properties { get; set; } = new();

    [Required]
    public bool SubscriptionPolicy { get; set; }

    [Required]
    public bool ThreeTenets { get; set; }
}
