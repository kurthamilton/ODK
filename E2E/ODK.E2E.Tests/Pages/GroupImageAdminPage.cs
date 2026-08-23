using Microsoft.Playwright;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel group picture page (<c>/my/groups/{chapterId}/image</c>). A group is created
/// without a picture and needs one before it can be published.
/// </summary>
internal class GroupImageAdminPage
{
    private readonly IPage _page;

    public GroupImageAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task SetPicture(Guid chapterId)
    {
        await _page.Navigate($"/my/groups/{chapterId}/image");

        // Uploading the file triggers the Cropper.js pipeline, which populates the hidden data URL
        // asynchronously - wait for it before submitting, or the form posts an empty one.
        await _page.SetInputFilesAsync("[data-img-input]", TestAssets.GroupImagePath);
        await _page.WaitForFunctionAsync(
            "() => { const el = document.querySelector('[data-img-dataurl]'); return !!el && el.value.length > 0; }");

        await _page.ClickAsync($"form[action='/groups/{chapterId}/image'] button");
        await _page.WaitForLoadStateAsync();
    }
}
