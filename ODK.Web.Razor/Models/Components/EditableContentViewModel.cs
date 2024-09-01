using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class EditableContentViewModel : EditableContentViewModelBase
{
    public required Func<object?, IHtmlContent> EditContentFunc { get; init; }

    public required Func<object?, IHtmlContent> ViewContentFunc { get; init; }
}
