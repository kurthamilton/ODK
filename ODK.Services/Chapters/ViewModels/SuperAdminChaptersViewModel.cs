using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class SuperAdminChaptersViewModel
{
    public required IReadOnlyCollection<Chapter> PendingApproval { get; set; }
}
