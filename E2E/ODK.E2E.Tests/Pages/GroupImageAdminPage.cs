using Microsoft.Playwright;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The picture panel of the Group Squirrel group settings page
/// (<c>/my/groups/{slug}/settings#picture</c>). A group is created without a picture and needs one
/// before it can be published.
/// </summary>
internal class GroupImageAdminPage
{
    private readonly IPage _page;

    public GroupImageAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task SetPicture(TestGroup group)
    {
        await _page.Navigate($"/my/groups/{group.Slug}/settings");

        /* Every selector is scoped to the picture panel: the settings page shows a site admin a second
           image cropper for the header image, and an unscoped selector would be ambiguous. */
        // The form is in a modal, so it has to be opened first - nothing inside a hidden modal is
        // clickable, whatever Playwright waits for.
        await _page.ClickAsync("#picture [data-odk-component='_ChapterAdminImage'] [data-bs-toggle='modal']");

        // Uploading the file triggers the Cropper.js pipeline, which populates the hidden data URL
        // asynchronously - wait for it before submitting, or the form posts an empty one.
        await _page.SetInputFilesAsync("#picture [data-img-input]", TestAssets.GroupImagePath);
        await _page.WaitForFunctionAsync(
            "() => { const el = document.querySelector('#picture [data-img-dataurl]'); "
            + "return !!el && el.value.length > 0; }");

        await _page.ClickAndWaitForDocument(
            $"#picture form[action='/groups/{group.ChapterId}/image'] button");
    }
}
