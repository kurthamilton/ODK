using System.ComponentModel.DataAnnotations;

namespace ODK.Core.Chapters;

public enum ChapterFeatureVisibilityType
{
    None = 0,
    Public = 1,
    [Display(Name = "All members")]
    AllMembers = 2,
    [Display(Name = "Active members")]
    ActiveMembers = 3
}
