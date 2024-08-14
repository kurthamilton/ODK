using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class EditableFormViewModel : EditableContentViewModelBase
{
    public Func<object?, IHtmlContent>? ButtonContent { get; init; }

    public required Func<object?, IHtmlContent> FormContent { get; init; }

    public required Func<object?, IHtmlContent> TextContent { get; init; }
}
