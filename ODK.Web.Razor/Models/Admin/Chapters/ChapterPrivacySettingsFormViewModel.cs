using System.ComponentModel;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterPrivacySettingsFormViewModel
{
    [DisplayName("Who can start conversations")]
    public ChapterFeatureVisibilityType? Conversations { get; set; }

    [DisplayName("Who can respond to events")]
    public ChapterFeatureVisibilityType? EventResponseVisibility { get; set; }

    [DisplayName("Who can see events")]
    public ChapterFeatureVisibilityType? EventVisibility { get; set; }

    [DisplayName("Who can see members")]
    public ChapterFeatureVisibilityType? MemberVisibility { get; set; }

    [DisplayName("Who can see event venues")]
    public ChapterFeatureVisibilityType? VenueVisibility { get; set; }
}
