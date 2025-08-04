using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class SuperAdminChaptersViewModel
{
    public required IReadOnlyCollection<SuperAdminChaptersRowViewModel> Approved { get; init; }

    public required IReadOnlyCollection<SuperAdminChaptersRowViewModel> Pending { get; init; }
}
