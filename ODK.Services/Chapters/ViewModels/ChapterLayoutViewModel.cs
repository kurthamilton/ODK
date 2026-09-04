using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterLayoutViewModel
{
    /// <summary>
    /// Version of the chapter's header image, or null where it has none. The version alone is what the
    /// header needs - the image itself is fetched by the browser from its own endpoint.
    /// </summary>
    public required int? HeaderImageVersion { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required IReadOnlyCollection<ChapterPage> Pages { get; init; }
}