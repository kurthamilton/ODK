using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class MemberChaptersViewModel
{
    public required IReadOnlyCollection<Chapter> Admin { get; set; }

    public required IReadOnlyCollection<Chapter> Member { get; set; }

    public required IReadOnlyCollection<Chapter> Owned { get; set; }
}
