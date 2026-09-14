namespace ODK.Web.Razor.Models.Components;

/// <summary>
/// The call to action that starts a group, whose destination depends on whether the visitor has an account
/// yet. See <c>Components/_StartGroupLink</c>.
/// </summary>
public class StartGroupLinkViewModel
{
    public string? Class { get; init; }

    public IconType? Icon { get; init; }

    public required string Text { get; init; }
}
