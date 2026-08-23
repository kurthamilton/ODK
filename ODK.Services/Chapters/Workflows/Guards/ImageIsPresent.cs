using ODK.Core.Workflows;

namespace ODK.Services.Chapters.Workflows.Guards;

/// <summary>
/// Whether the group has the picture publication requires. The picture is shown wherever a group is
/// listed, so a group without one cannot be presented to anyone looking for it.
/// </summary>
public sealed class ImageIsPresent : IGuard<ChapterPublicationContext>
{
    public string Description => "with a picture";

    public bool IsSatisfied(ChapterPublicationContext context) => context.RequiredHasImage;
}
