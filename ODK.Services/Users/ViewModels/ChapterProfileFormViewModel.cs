using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class ChapterProfileFormViewModel : ChapterProfileFormSubmitViewModel
{
    public required string ChapterName { get; init; }

    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; }

    public required IReadOnlyCollection<ChapterPropertyOption> ChapterPropertyOptions { get; init; }

    /// <summary>
    /// Whether the form is part of raising an account, which is what the profile picture and the privacy
    /// policy belong to rather than to the group.
    /// </summary>
    /// <remarks>
    /// Not the same question as whether anyone is signed in, which is what it used to be read from: an
    /// invitation is answered by a visitor with no session whose account already exists, so that form asks
    /// for the group's questions and nothing else.
    /// </remarks>
    public required bool SigningUp { get; init; }
}
