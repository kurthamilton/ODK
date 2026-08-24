namespace ODK.Web.Razor.Models.Components;

/// <summary>
/// Which heading element a title renders as. Chosen by the caller rather than fixed by the component that
/// holds the title, because the right level depends on what the surrounding page already uses.
/// </summary>
public enum HeadingType
{
    None,
    H1,
    H2,
    H3,
    H4,
    H5,
    H6
}