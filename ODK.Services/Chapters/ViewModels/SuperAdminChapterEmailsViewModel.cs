using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class SuperAdminChapterEmailsViewModel
{
    public required IReadOnlyCollection<ChapterEmailProvider> EmailProviders { get; init; }
}
